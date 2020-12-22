using AppConfigSyncFunction.Events;
using AppConfigSyncFunction.Interfaces;
using AppConfigSyncFunction.Logging;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppConfigSyncFunction.Services
{
    public class StorageQueueEventsService : IEventsService
    {
        public class MessageIdentifier
        {
            public string MessageId { get; set; }
            public string PopReceipt { get; set; }
        }

        public const string DefaultStorageConnectionStringKey = "SyncQueueConnectionString";
        public const string DefaultQueueName = "syncqueue";

        private QueueClient queueClient;

        private readonly ILogger<StorageQueueEventsService> logger;
        private readonly IConfiguration configuration;

        public StorageQueueEventsService(ILogger<StorageQueueEventsService> logger,
            IConfiguration configuration)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            this.logger = logger;
            this.configuration = configuration;
        }

        public async Task ConnectAsync(string storageConnectionStringKey = null,
            string queueName = null)
        {
            logger.LogTrace($"Starting creation Storage Queue client");
            storageConnectionStringKey ??= DefaultStorageConnectionStringKey;
            string storageConnectionString = configuration.GetValue<string>(storageConnectionStringKey);
            if (string.IsNullOrWhiteSpace(storageConnectionString))
            {
                var message = $"The connection string '{storageConnectionStringKey}' is not valid";
                logger.LogError(message);
                throw new Exception(message);
            }

            queueName ??= DefaultQueueName;
            queueClient = new QueueClient(storageConnectionString, queueName); ;

            await queueClient.CreateIfNotExistsAsync();

            logger.LogTrace($"Finish creation Storage Queue client");
        }

        public async Task<IEnumerable<Event>> ReceiveEventsAsync(int numberOfEvents)
        {
            if (numberOfEvents <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfEvents));

            logger.LogTrace($"Starting {nameof(ReceiveEventsAsync)}");

            IEnumerable<Event> result = null;
            try
            {
                using (var metricLogger = new DurationMetricLogger(MetricNames.RetrieveMessagesDuration, logger))
                {
                    var response = await queueClient.ReceiveMessagesAsync(numberOfEvents);
                    if (response.HasMessages())
                        result = response.Value.Select(m =>
                        {
                            var e = m.CreateEvent();
                            e.PersistenceId = new MessageIdentifier()
                            {
                                MessageId = m.MessageId,
                                PopReceipt = m.PopReceipt
                            };
                            return e;
                        }).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error during retrieving messages from queue");
                result = null;
            }

            logger.LogTrace($"Finish {nameof(ReceiveEventsAsync)}");

            return result;
        }

        public async Task DeleteEventAsync(Event @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));
            if (@event.PersistenceId == null)
                throw new ArgumentException(nameof(@event.PersistenceId));

            logger.LogTrace($"Starting {nameof(DeleteEventAsync)}");

            try
            {
                var messageId = @event.PersistenceId as MessageIdentifier;

                using (var metricLogger = new DurationMetricLogger(MetricNames.DeleteMessageDuration, logger))
                {
                    await queueClient.DeleteMessageAsync(messageId.MessageId, messageId.PopReceipt);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error during deleting message from queue");
                throw;
            }

            logger.LogTrace($"Finish {nameof(DeleteEventAsync)}");
        }
    }
}
