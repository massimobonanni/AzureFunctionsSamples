using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AppConfigSyncFunction
{
    public class Event
    {
        public const string KeyValueDeleted = "Microsoft.AppConfiguration.KeyValueDeleted";
        public const string KeyValueModified = "Microsoft.AppConfiguration.KeyValueModified";

        public string Id { get; set; }
        public string Topic { get; set; }
        public string Subject { get; set; }
        public Data Data { get; set; }
        public string EventType { get; set; }
        public string DataVersion { get; set; }
        public string MetadataVersion { get; set; }
        public DateTime EventTime { get; set; }

        public bool IsKeyValueModified()
        {
            return this.EventType == KeyValueModified;
        }

        public bool IsKeyValueDeleted()
        {
            return this.EventType == KeyValueDeleted;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
