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
        private readonly IQueueService queueService;
        private readonly IConfiguration configuration;

        public AppConfigSync(IAppConfigurationSyncService appConfigSyncService,
            IQueueService queueService,
            IConfiguration configuration)
        {
            if (appConfigSyncService == null)
                throw new ArgumentNullException(nameof(appConfigSyncService));
            if (queueService == null)
                throw new ArgumentNullException(nameof(queueService));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.appConfigSyncService = appConfigSyncService;
            this.queueService = queueService;
            this.configuration = configuration;
        }

        [FunctionName(nameof(AppConfigSyncFunction))]
        public async Task AppConfigSyncFunction([TimerTrigger("%UpdatePollingTime%")] TimerInfo timer,
            ILogger log)
        {
            await queueService.ConnectAsync();
            IEnumerable<QueueMessage> messages;
            do
            {
                messages = await queueService.ReceiveMessagesAsync(30);
                if (messages != null && messages.Any())
                {
                    appConfigSyncService.Connect();
                    bool syncResult = true;
                    foreach (var message in messages)
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
                            await queueService.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                    }
                }
            } while (messages != null && messages.Any());

        }
    }
}
