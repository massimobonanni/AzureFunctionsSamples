using AppConfigSyncFunction.Interfaces;
using AppConfigSyncFunction.Logging;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppConfigSyncFunction.Services
{
    public class StorageQueueService : IQueueService
    {

        public const string DefaultStorageConnectionStringKey = "SyncQueueConnectionString";
        public const string DefaultQueueName = "syncqueue";

        private QueueClient queueClient;

        private readonly ILogger<StorageQueueService> logger;
        private readonly IConfiguration configuration;

        public StorageQueueService(ILogger<StorageQueueService> logger,
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

        public async Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int numberOfMessages)
        {
            if (numberOfMessages <= 0)
                throw new ArgumentOutOfRangeException(nameof(numberOfMessages));

            logger.LogTrace($"Starting {nameof(ReceiveMessagesAsync)}");

            IEnumerable<QueueMessage> result = null;
            try
            {
                using (var metricLogger = new DurationMetricLogger(MetricNames.RetrieveMessagesDuration, logger))
                {
                    var response = await queueClient.ReceiveMessagesAsync(numberOfMessages);
                    if (response.HasMessages())
                        result = response.Value;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error during retrieving messages from queue");
                result = null;
            }

            logger.LogTrace($"Finish {nameof(ReceiveMessagesAsync)}");

            return result;
        }

        public async Task DeleteMessageAsync(string messageId, string popReceipt)
        {
            if (string.IsNullOrWhiteSpace(messageId))
                throw new ArgumentException(nameof(messageId));
            if (string.IsNullOrWhiteSpace(popReceipt))
                throw new ArgumentException(nameof(popReceipt));

            logger.LogTrace($"Starting {nameof(DeleteMessageAsync)}");

            try
            {
                using (var metricLogger = new DurationMetricLogger(MetricNames.DeleteMessageDuration, logger))
                {
                    await queueClient.DeleteMessageAsync(messageId, popReceipt);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error during deleting message from queue");
                throw;
            }

            logger.LogTrace($"Finish {nameof(DeleteMessageAsync)}");
        }
    }
}
