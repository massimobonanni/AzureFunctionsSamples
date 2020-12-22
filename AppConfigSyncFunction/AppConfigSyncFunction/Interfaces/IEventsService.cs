using AppConfigSyncFunction.Events;
using Azure.Storage.Queues.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AppConfigSyncFunction.Interfaces
{
    public interface IEventsService
    {
        Task ConnectAsync(string storageConnectionStringKey = null, string queueName = null);
        Task<IEnumerable<Event>> ReceiveEventsAsync(int numberOfEvents);
        Task DeleteEventAsync(Event @event);
    }
}
