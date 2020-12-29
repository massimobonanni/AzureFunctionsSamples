using AppConfigSyncFunction.Events;
using Azure.Storage.Queues.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppConfigSyncFunction.Tests
{
    internal static class TestUtility
    {
        internal static TimerInfo GenerateTimerInfo()
        {
            return new TimerInfo(null, new ScheduleStatus());
        }

        internal static Event CreateKeyValueModifiedEvent()
        {
            return new Event()
            {
                EventTime = DateTime.Now,
                EventType = Event.KeyValueModified,
                Id = Guid.NewGuid().ToString(),
                DataVersion = "1",
                MetadataVersion = "1",
                Subject = "https://primaryappconfig.azconfig.io/kv/myConfigValue?label=%00&api-version=1.0",
                Topic = "/subscriptions/60504780-44c1-4d01-b84f-baca1d9239ed/resourceGroups/appconfigurationreplication-rg/providers/microsoft.appconfiguration/configurationstores/primaryappconfig",
                Data = new Data()
                {
                    etag = Guid.NewGuid().ToString(),
                    key = "Key",
                    label = "label"
                }
            };
        }

        internal static Event CreateKeyValueDeletedEvent()
        {
            return new Event()
            {
                EventTime = DateTime.Now,
                EventType = Event.KeyValueDeleted,
                Id = Guid.NewGuid().ToString(),
                DataVersion = "1",
                MetadataVersion = "1",
                Subject = "https://primaryappconfig.azconfig.io/kv/myConfigValue?label=%00&api-version=1.0",
                Topic = "/subscriptions/60504780-44c1-4d01-b84f-baca1d9239ed/resourceGroups/appconfigurationreplication-rg/providers/microsoft.appconfiguration/configurationstores/primaryappconfig",
                Data = new Data()
                {
                    etag = Guid.NewGuid().ToString(),
                    key = "Key",
                    label = "label"
                }
            };
        }
    }
}
