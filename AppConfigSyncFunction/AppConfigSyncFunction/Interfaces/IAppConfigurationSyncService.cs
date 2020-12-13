using AppConfigSyncFunction.Events;
using System.Threading;
using System.Threading.Tasks;

namespace AppConfigSyncFunction.Interfaces
{
    public interface IAppConfigurationSyncService
    {
        void Connect(string primaryConnectionString = null, string secondaryConnectionString = null);
        Task<bool> DeleteFromSecondary(Event @event, CancellationToken cancellationToken = default);
        Task<bool> UpsertToSecondary(Event @event, CancellationToken cancellationToken = default);
    }
}