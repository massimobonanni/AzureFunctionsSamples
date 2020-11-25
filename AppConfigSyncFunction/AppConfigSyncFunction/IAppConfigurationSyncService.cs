using System.Threading;
using System.Threading.Tasks;

namespace AppConfigSyncFunction
{
    public interface IAppConfigurationSyncService
    {
        void Connect(string primaryConnectionString = "AppConfigPrimaryConnectionString", string secondaryConnectionString = "AppConfigSecondaryConnectionString");
        Task<bool> DeleteFromSecondary(Event @event, CancellationToken cancellationToken = default);
        Task<bool> UpsertToSecondary(Event @event, CancellationToken cancellationToken = default);
    }
}