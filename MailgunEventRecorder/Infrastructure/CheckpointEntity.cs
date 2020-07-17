namespace MailgunEventRecorder.Infrastructure
{
    using System;
    using Microsoft.Azure.Cosmos.Table;

    public class CheckpointEntity : TableEntity
    {
        public CheckpointEntity()
        {
        }

        public CheckpointEntity(DateTime utcDate)
        {
            this.UtcDate = utcDate;
            this.PartitionKey = this.RowKey = nameof(CheckpointEntity);
        }

        public DateTime UtcDate { get; set; }
    }
}
