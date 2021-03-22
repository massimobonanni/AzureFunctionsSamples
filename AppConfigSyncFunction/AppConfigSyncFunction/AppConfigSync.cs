using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppConfigSyncFunction.Events;
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
        private readonly IEventsService queueService;
        private readonly IConfiguration configuration;

        public AppConfigSync(IAppConfigurationSyncService appConfigSyncService,
            IEventsService queueService,
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
        public async Task AppConfigSyncFunction(
            [TimerTrigger("%UpdatePollingTime%")] TimerInfo timer,
            ILogger log)
        {
            await queueService.ConnectAsync();
            IEnumerable<Event> events;
            do
            {
                events = await queueService.ReceiveEventsAsync(30);
                if (events != null && events.Any())
                {
                    appConfigSyncService.Connect();
                    bool syncResult = true;
                    foreach (var @event in events)
                    {
                        using (var metricLogger = new DurationMetricLogger(MetricNames.EventSynchronizationDuration, log))
                        {
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
                            await queueService.DeleteEventAsync(@event);
                    }
                }
            } while (events != null && events.Any());

        }
    }
}
