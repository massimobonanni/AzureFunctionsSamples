namespace AppConfigSyncFunction.Logging
{
    public static class MetricNames
    {
        public const string UpsertToSecondaryDuration = "UpsertToSecondaryDuration";
        public const string DeleteFromSecondaryDuration = "DeleteFromSecondaryDuration";

        public const string EventSynchronizationDuration = "EventSynchronizationDuration";

        public const string RetrieveMessagesDuration= "RetrieveMessagesDuration";
        public const string DeleteMessageDuration = "DeleteMessageDuration";
    }
}
