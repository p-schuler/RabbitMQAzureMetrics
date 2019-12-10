using Microsoft.ApplicationInsights;
using RabbitMQAzureMetrics.MetricsValueConverters;
using System;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.ValuePublishers.AppInsight
{
    public class TelemetryClientPublisher : IMetricPublisher
    {
        private readonly Metric metric;

        public TelemetryClientPublisher(Metric metric)
        {
            this.metric = metric;
        }

        public Task PublishAsync(MetricValueCollectionWrapper collector)
        {
            var values = collector.Values;
            if (values.Count <= 0) return Task.CompletedTask;

            var numberOfDimensions = values[0].Dimensions.Length;

            var types = new Type[1 + numberOfDimensions];
            types[0] = typeof(double);
            for (var i = 0; i < numberOfDimensions; i++) types[i+1] = typeof(string);
            var method = this.metric.GetType().GetMethod(nameof(metric.TrackValue), types);
            var parameters = new object[types.Length];

            foreach (var itm in collector.Values)
            {
                parameters[0] = itm.Value; // boxing
                for (var i = 0; i < numberOfDimensions; i++) parameters[i+1] = itm.Dimensions[i];
                method.Invoke(this.metric, parameters);
            }
            return Task.CompletedTask;
        }
    }
}
