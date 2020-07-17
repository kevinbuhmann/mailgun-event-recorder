namespace MailgunEventRecorder.Infrastructure
{
    using System;
    using System.Net.Http;
    using Polly;

    public static class HttpPolicyMaker
    {
        private static readonly Random Random = new Random();

        public static IAsyncPolicy<HttpResponseMessage> GetHttpPolicy()
        {
            return Policy.WrapAsync(
                GetRetryPolicy(),
                GetBulkheadPolicy(),
                GetTimeoutPolicy(),
                GetCircuitBreakerPolicy());
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            Random jitterer = new Random();

            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(4, GetTimeToSleep);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetBulkheadPolicy()
        {
            return Policy.BulkheadAsync<HttpResponseMessage>(maxParallelization: 10, maxQueuingActions: 100);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(seconds: 30);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .AdvancedCircuitBreakerAsync(
                    failureThreshold: 0.9,
                    samplingDuration: TimeSpan.FromMinutes(1),
                    minimumThroughput: 10,
                    durationOfBreak: TimeSpan.FromMinutes(1));
        }

        private static TimeSpan GetTimeToSleep(int retryAttempt)
        {
            TimeSpan exponentialBackoff = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
            TimeSpan jitter = TimeSpan.FromMilliseconds(Random.Next(0, 1000));

            return exponentialBackoff + jitter;
        }
    }
}
