namespace MailgunEventRecorder.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMailgunEventRetriever
    {
        Task<IReadOnlyCollection<string>> GetEvents(string domain, DateTime utcFrom, DateTime utcTo);
    }
}
