using System.Threading.Tasks;

namespace SharedServices
{
    public interface IEventLogger
    {
        Task LogEvent(string eventName, string user = null);
    }
}
