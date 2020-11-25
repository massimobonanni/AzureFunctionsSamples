using Azure;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;

namespace AppConfigSyncFunction
{
    internal static class Extensions
    {
        public static Event CreateEvent(this QueueMessage message)
        {
            if (message == null)
                throw new NullReferenceException(nameof(message));

            var base64EncodedBytes = Convert.FromBase64String(message.MessageText);
            return JsonConvert.DeserializeObject<Event>(Encoding.UTF8.GetString(base64EncodedBytes));
        }

        public static bool IsValid(this Response<QueueMessage[]> response)
        {
            return response != null && response.Value != null && response.Value.Any();
        }
    }
}
