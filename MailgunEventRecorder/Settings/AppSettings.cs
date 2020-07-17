namespace MailgunEventRecorder.Settings
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Configuration;

    public class AppSettings
    {
        public AppSettings(IConfiguration configuration)
        {
            this.CheckpointPeriod = TimeSpan.FromSeconds(GetRequiredValue<int>(configuration, "CheckpointPeriodInSeconds"));
            this.TimerPeriod = TimeSpan.FromSeconds(GetRequiredValue<int>(configuration, "TimerPeriodInSeconds"));
            this.MailgunApiKey = GetRequiredValue<string>(configuration, "MailgunApiKey");
            this.MailgunSendingDomains = GetRequiredValue<string>(configuration, "MailgunSendingDomains").Split(',').ToList();
            this.MailgunAccountEventStoragePeriod = TimeSpan.FromDays(GetRequiredValue<int>(configuration, "MailgunAccountEventStoragePeriodInDays"));
            this.MailgunEventLagTime = TimeSpan.FromSeconds(GetRequiredValue<int>(configuration, "MailgunEventLagTimeInSeconds"));
            this.AzureStorageConnectionString = GetRequiredValue<string>(configuration, "AzureStorageConnectionString");
            this.EventHubConnectionString = GetRequiredValue<string>(configuration, "EventHubConnectionString");
        }

        public TimeSpan CheckpointPeriod { get; }

        public TimeSpan TimerPeriod { get; }

        public string MailgunApiKey { get; }

        public IReadOnlyCollection<string> MailgunSendingDomains { get; }

        public TimeSpan MailgunAccountEventStoragePeriod { get; }

        public TimeSpan MailgunEventLagTime { get; }

        public string AzureStorageConnectionString { get; }

        public string EventHubConnectionString { get; }

        private static T GetRequiredValue<T>(IConfiguration configuration, string key)
        {
            if (configuration[key] == null)
            {
                throw new Exception($"Configuration \"{key}\" is required.");
            }

            return configuration.GetValue<T>(key);
        }
    }
}
