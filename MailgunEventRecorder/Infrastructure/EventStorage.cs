namespace MailgunEventRecorder.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Azure.Messaging.EventHubs;
    using Azure.Messaging.EventHubs.Producer;
    using MailgunEventRecorder.Contracts;
    using MailgunEventRecorder.Settings;

    public class EventStorage : IEventStorage
    {
        private readonly EventHubProducerClient client;

        public EventStorage(AppSettings appSettings)
        {
            this.client = new EventHubProducerClient(appSettings.EventHubConnectionString);
        }

        public async Task StoreEvents(IReadOnlyCollection<string> events)
        {
            List<EventDataBatch> batches = new List<EventDataBatch>();
            EventDataBatch? currentBatch = null;

            foreach (string @event in events)
            {
                EventData eventData = GetEventData(@event);

                if (currentBatch?.TryAdd(eventData) != true)
                {
                    currentBatch = await this.client.CreateBatchAsync();
                    batches.Add(currentBatch);

                    if (currentBatch.TryAdd(eventData) == false)
                    {
                        throw new Exception("Event is too large for empty batch");
                    }
                }
            }

            await Task.WhenAll(batches.Select(batch => this.client.SendAsync(batch)));

            foreach (EventDataBatch batch in batches)
            {
                batch.Dispose();
            }
        }

        private static EventData GetEventData(string data)
        {
            return new EventData(Encoding.UTF8.GetBytes(data));
        }
    }
}
