namespace MailgunEventRecorder.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using MailgunEventRecorder.Contracts;
    using MailgunEventRecorder.Helpers;
    using MailgunEventRecorder.Settings;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public sealed class EventRecorder : IHostedService, IDisposable
    {
        private readonly ILogger<EventRecorder> logger;
        private readonly AppSettings appSettings;
        private readonly ICheckpointStorage dateStorage;
        private readonly IMailgunEventRetriever mailgunEventRetriever;
        private readonly IEventStorage eventStorage;

        private Timer? timer = default;

        public EventRecorder(
            ILogger<EventRecorder> logger,
            AppSettings appSettings,
            ICheckpointStorage dateStorage,
            IMailgunEventRetriever mailgunEventRetriever,
            IEventStorage eventStorage)
        {
            this.logger = logger;
            this.appSettings = appSettings;
            this.dateStorage = dateStorage;
            this.mailgunEventRetriever = mailgunEventRetriever;
            this.eventStorage = eventStorage;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Start");

            this.timer = new Timer(
                callback: this.DoWork,
                state: null,
                dueTime: TimeSpan.Zero,
                period: this.appSettings.TimerPeriod);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stop");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.timer?.Dispose();
        }

        private async void DoWork(object? state)
        {
            this.logger.LogInformation("Work");

            DateTime utcLastCheckpoint = await this.GetLastProcessedDate();
            DateTime finalUtcEnd = DateTime.UtcNow.Subtract(this.appSettings.MailgunEventLagTime);

            while (utcLastCheckpoint < finalUtcEnd)
            {
                DateTime utcNextCheckpoint = DateHelpers.MinDate(utcLastCheckpoint.Add(this.appSettings.CheckpointPeriod), finalUtcEnd);

                await this.ProcessCheckpoint(utcLastCheckpoint: utcLastCheckpoint, utcNextCheckpoint: utcNextCheckpoint);

                utcLastCheckpoint = utcNextCheckpoint;
            }
        }

        private async Task ProcessCheckpoint(DateTime utcLastCheckpoint, DateTime utcNextCheckpoint)
        {
            IEnumerable<Task> tasks = this.appSettings.MailgunSendingDomains
                .Select(async domain =>
                {
                    IReadOnlyCollection<string> events = await this.mailgunEventRetriever.GetEvents(domain: domain, utcFrom: utcLastCheckpoint, utcTo: utcNextCheckpoint);

                    if (events.Any())
                    {
                        await this.eventStorage.StoreEvents(events);
                    }
                });

            await Task.WhenAll(tasks);

            await this.dateStorage.StoreLastProcessedDate(utcNextCheckpoint);
        }

        private async Task<DateTime> GetLastProcessedDate()
        {
            return DateHelpers.MaxDate(
                date1: await this.dateStorage.GetLastProcessedDate() ?? DateTime.MinValue,
                date2: DateTime.UtcNow.Subtract(this.appSettings.MailgunAccountEventStoragePeriod));
        }
    }
}
