using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppConfigSyncFunction.Interfaces
{
    public interface IQueueService
    {
        Task ConnectAsync(string storageConnectionStringKey = null, string queueName = null);
        Task<IEnumerable<QueueMessage>> ReceiveMessagesAsync(int numberOfMessages);
        Task DeleteMessageAsync(string messageId, string popReceipt);
    }
}
