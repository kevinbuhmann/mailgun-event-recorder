namespace MailgunEventRecorder.Contracts
{
    using System;
    using System.Threading.Tasks;

    public interface ICheckpointStorage
    {
        Task<DateTime?> GetLastProcessedDate();

        Task StoreLastProcessedDate(DateTime utcCheckpoint);
    }
}
