using System;
using System.Collections.Generic;
using System.Text;

namespace AppConfigSyncFunction
{
    public static class MetricNames
    {
        public const string UpsertToSecondaryDuration = "UpsertToSecondaryDuration";
        public const string DeleteFromSecondaryDuration = "DeleteFromSecondaryDuration";

        public const string EventSynchronizationDuration = "EventSynchronizationDuration";
    }
}
