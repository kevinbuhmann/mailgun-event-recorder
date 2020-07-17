namespace MailgunEventRecorder.Contracts
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IEventStorage
    {
        Task StoreEvents(IReadOnlyCollection<string> events);
    }
}
