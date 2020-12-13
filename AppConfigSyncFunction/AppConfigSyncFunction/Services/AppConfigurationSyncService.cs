using AppConfigSyncFunction.Events;
using AppConfigSyncFunction.Interfaces;
using AppConfigSyncFunction.Logging;
using Azure.Data.AppConfiguration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppConfigSyncFunction.Services
{
    public class AppConfigurationSyncService : IAppConfigurationSyncService
    {
        public const string DefaultPrimaryConnectionStringKey = "AppConfigPrimaryConnectionString";
        public const string DefaultSecondaryConnectionStringKey = "AppConfigSecondaryConnectionString";

        private ConfigurationClient primaryClient;
        private ConfigurationClient secondaryClient;
        private readonly ILogger<AppConfigurationSyncService> logger;
        private readonly IConfiguration configuration;
        public AppConfigurationSyncService(ILogger<AppConfigurationSyncService> logger,
            IConfiguration configuration)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.logger = logger;
            this.configuration = configuration;
        }

        public void Connect(string primaryConnectionString = DefaultPrimaryConnectionStringKey,
            string secondaryConnectionString = DefaultSecondaryConnectionStringKey)
        {
            logger.LogTrace($"Starting creation App Configuration clients");

            string appConfigPrimaryConnectionString = configuration.GetValue<string>(primaryConnectionString);
            if (string.IsNullOrWhiteSpace(appConfigPrimaryConnectionString))
            {
                var message = $"The connection string '{primaryConnectionString}' is not valid";
                logger.LogError(message);
                throw new Exception(message);
            }

            string appConfigSecondaryConnectionString = configuration.GetValue<string>(secondaryConnectionString);
            if (string.IsNullOrWhiteSpace(appConfigSecondaryConnectionString))
            {
                var message = $"The connection string '{appConfigSecondaryConnectionString}' is not valid";
                logger.LogError(message);
                throw new Exception(message);
            }

            primaryClient = new ConfigurationClient(appConfigPrimaryConnectionString);
            secondaryClient = new ConfigurationClient(appConfigSecondaryConnectionString);

            logger.LogTrace($"Finish creation App Configuration clients");
        }

        public async Task<bool> UpsertToSecondary(Event @event, CancellationToken cancellationToken = default)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            logger.LogTrace($"Starting {nameof(UpsertToSecondary)}");

            var result = true;
            try
            {
                using (var metricLogger = new DurationMetricLogger(MetricNames.UpsertToSecondaryDuration, logger))
                {
                    var primarySetting = await primaryClient.GetConfigurationSettingAsync(@event.Data.key, @event.Data.label, cancellationToken);
                    await secondaryClient.SetConfigurationSettingAsync(primarySetting, false, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error during upsert settings from primary to secondary -> [{@event.Data.key},{@event.Data.label}]");
                result = false;
            }

            logger.LogTrace($"Finish {nameof(UpsertToSecondary)}");

            return result;
        }

        public async Task<bool> DeleteFromSecondary(Event @event, CancellationToken cancellationToken = default)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            logger.LogTrace($"Starting {nameof(DeleteFromSecondary)}");

            var result = true;
            try
            {
                using (var metricLogger = new DurationMetricLogger(MetricNames.DeleteFromSecondaryDuration, logger))
                {
                    await secondaryClient.DeleteConfigurationSettingAsync(@event.Data.key, @event.Data.label, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error during delete settings from secondary -> [{@event.Data.key},{@event.Data.label}]");
                result = false; ;
            }

            logger.LogTrace($"Finish {nameof(DeleteFromSecondary)}");

            return result;
        }
    }
}
