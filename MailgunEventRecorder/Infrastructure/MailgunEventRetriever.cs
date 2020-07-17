namespace MailgunEventRecorder.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using MailgunEventRecorder.Contracts;
    using MailgunEventRecorder.Helpers;
    using MailgunEventRecorder.Settings;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class MailgunEventRetriever : IMailgunEventRetriever
    {
        private readonly HttpClient httpClient;
        private readonly AppSettings appSettings;

        public MailgunEventRetriever(
            HttpClient httpClient,
            AppSettings appSettings)
        {
            this.httpClient = httpClient;
            this.appSettings = appSettings;
        }

        public async Task<IReadOnlyCollection<string>> GetEvents(string domain, DateTime utcFrom, DateTime utcTo)
        {
            List<string> events = new List<string>();
            Uri? requestUri = GetRequestUrl(domain: domain, utcFrom: utcFrom, utcTo: utcTo);

            do
            {
                using (HttpRequestMessage request = this.GetHttpRequest(requestUri))
                using (HttpResponseMessage response = await this.httpClient.SendAsync(request))
                {
                    string responseContent = await response.Content.ReadAsStringAsync();

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        JToken token = JsonConvert.DeserializeObject<JToken>(responseContent);
                        IReadOnlyCollection<string> batchEvents = GetEventsFromResponse(token);

                        if (batchEvents.Any() == true)
                        {
                            events.AddRange(batchEvents);
                            requestUri = GetNextPageUrlFromResponse(token);
                        }
                        else
                        {
                            requestUri = null;
                        }
                    }
                    else
                    {
                        throw new Exception($"Failed HTTP Call: {responseContent}");
                    }
                }
            }
            while (requestUri != null);

            return events;
        }

        private static Uri GetRequestUrl(string domain, DateTime utcFrom, DateTime utcTo)
        {
            return new Uri($"https://api.mailgun.net/v3/{domain}/events?ascending=yes&begin={DateHelpers.ToUnixTimeStamp(utcFrom)}&end={DateHelpers.ToUnixTimeStamp(utcTo)}");
        }

        private static IReadOnlyCollection<string> GetEventsFromResponse(JToken token)
        {
            JArray events = (JArray?)token.SelectToken("items") ?? new JArray();

            return events.Select(@event => @event.ToString()).ToList();
        }

        private static Uri? GetNextPageUrlFromResponse(JToken token)
        {
            string? url = token.SelectToken("paging.next")?.Value<string>();

            return url == null ? null : new Uri(url);
        }

        private HttpRequestMessage GetHttpRequest(Uri requestUri)
        {
            HttpRequestMessage request = new HttpRequestMessage(method: HttpMethod.Get, requestUri: requestUri);

            request.Headers.Authorization = new AuthenticationHeaderValue(
                scheme: "Basic",
                parameter: Convert.ToBase64String(Encoding.UTF8.GetBytes($"api:{this.appSettings.MailgunApiKey}")));

            return request;
        }
    }
}
