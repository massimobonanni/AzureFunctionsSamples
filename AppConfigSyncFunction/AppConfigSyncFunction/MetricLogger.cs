using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace AppConfigSyncFunction
{
    public abstract class MetricLoggerBase : IDisposable
    {
        protected readonly ILogger logger;
        public MetricLoggerBase(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            this.logger = logger;
        }

        public virtual void Dispose()
        {

        }
    }

    public class DurationMetricLogger : MetricLoggerBase
    {
        private Stopwatch stopwatch;
        private string metricName;
        private IDictionary<string, object> properties=new Dictionary<string, object>();

        public DurationMetricLogger(string metricName, ILogger logger,
            IDictionary<string, object> properties = null) : base(logger)
        {
            if (string.IsNullOrWhiteSpace(metricName))
                throw new ArgumentException(nameof(metricName));

            this.metricName = metricName;
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    this.properties.Add(prop);
                }
            }
            stopwatch = Stopwatch.StartNew();
        }

        public override void Dispose()
        {
            stopwatch.Stop();

            this.properties.Add(nameof(stopwatch.Elapsed), stopwatch.Elapsed);
            logger.LogMetric(metricName, stopwatch.ElapsedMilliseconds, this.properties);
            
            base.Dispose();
        }
    }
}
