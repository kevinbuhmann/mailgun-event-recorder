namespace MailgunEventRecorder.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using MailgunEventRecorder.Contracts;
    using MailgunEventRecorder.Settings;
    using Microsoft.Azure.Cosmos.Table;

    public class CheckpointStorage : ICheckpointStorage
    {
        private readonly AppSettings appSettings;
        private CloudTable? cloudTable = null;

        public CheckpointStorage(AppSettings appSettings)
        {
            this.appSettings = appSettings;
        }

        public async Task<DateTime?> GetLastProcessedDate()
        {
            await this.EnsureSetup();

            if (this.cloudTable!.Exists() == false)
            {
                return null;
            }

            TableResult result = await this.cloudTable.ExecuteAsync(TableOperation.Retrieve<CheckpointEntity>(
                partitionKey: nameof(CheckpointEntity),
                rowkey: nameof(CheckpointEntity)));

            return (result.Result as CheckpointEntity)?.UtcDate;
        }

        public async Task StoreLastProcessedDate(DateTime utcCheckpoint)
        {
            await this.EnsureSetup();

            await this.cloudTable!.ExecuteAsync(TableOperation.InsertOrReplace(new CheckpointEntity(utcCheckpoint)));
        }

        private async Task EnsureSetup()
        {
            if (this.cloudTable == null)
            {
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(this.appSettings.AzureStorageConnectionString);
                CloudTableClient tableClient = cloudStorageAccount.CreateCloudTableClient();
                CloudTable cloudTable = tableClient.GetTableReference("checkpoints");

                await cloudTable.CreateIfNotExistsAsync();

                this.cloudTable = cloudTable;
            }
        }
    }
}
