using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppConfigSyncFunction.Interfaces;
using AppConfigSyncFunction.Logging;
using Azure;
using Azure.Data.AppConfiguration;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AppConfigSyncFunction
{
    public class AppConfigSync
    {
        private readonly IAppConfigurationSyncService appConfigSyncService;
        private readonly IConfiguration configuration;

        public AppConfigSync(IAppConfigurationSyncService appConfigSyncService,
            IConfiguration configuration)
        {
            if (appConfigSyncService == null)
                throw new ArgumentNullException(nameof(appConfigSyncService));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.appConfigSyncService = appConfigSyncService;
            this.configuration = configuration;
        }

        [FunctionName(nameof(AppConfigSyncFunction))]
        public async Task AppConfigSyncFunction([TimerTrigger("%UpdatePollingTime%")] TimerInfo timer,
            ILogger log)
        {
            string queueConnectionString = configuration.GetValue<string>("SyncQueueConnectionString");
            var queueClient = new QueueClient(queueConnectionString, "syncqueue");
            if (queueClient.Exists())
            {
                Response<QueueMessage[]> response = null;
                do
                {
                    response = await queueClient.ReceiveMessagesAsync(30);
                    if (response.HasMessages())
                    {
                        appConfigSyncService.Connect();
                        bool syncResult = true;
                        foreach (var message in response.Value)
                        {
                            using (var metricLogger = new DurationMetricLogger(MetricNames.EventSynchronizationDuration, log))
                            {
                                var @event = message.CreateEvent();

                                syncResult = true;
                                if (@event.IsKeyValueModified()) // the setting was added or updated
                                {
                                    syncResult = await appConfigSyncService.UpsertToSecondary(@event);
                                }
                                else if (@event.IsKeyValueDeleted()) // the setting was deleted
                                {
                                    syncResult = await appConfigSyncService.DeleteFromSecondary(@event);
                                }
                            }

                            if (syncResult)
                                await queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                        }
                    }
                } while (response.HasMessages());
            }
        }
    }
}
