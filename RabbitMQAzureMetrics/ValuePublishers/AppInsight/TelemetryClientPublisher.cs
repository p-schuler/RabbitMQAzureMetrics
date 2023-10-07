using Microsoft.Extensions.Logging;

namespace RabbitMQAzureMetrics.ValuePublishers.AppInsight
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using RabbitMQAzureMetrics.MetricsValueConverters;

    public class TelemetryClientPublisher : IMetricPublisher
    {
        private readonly Metric metric;
        private readonly ILogger logger;

        public TelemetryClientPublisher(Metric metric, ILogger logger)
        {
            this.metric = metric;
            this.logger = logger;
        }

        public Task PublishAsync(MetricValueCollectionWrapper collector)
        {
            var values = collector.Values;
            if (values.Count <= 0)
            {
                return Task.CompletedTask;
            }

            var numberOfDimensions = values[0].Dimensions.Length;

            var types = new Type[1 + numberOfDimensions];
            types[0] = typeof(double);
            for (var i = 0; i < numberOfDimensions; i++)
            {
                types[i + 1] = typeof(string);
            }

            var method = this.metric.GetType().GetMethod(nameof(this.metric.TrackValue), types);
            var parameters = new object[types.Length];
            var result = true;

            foreach (var itm in collector.Values)
            {
                parameters[0] = itm.Value; // boxing
                for (var i = 0; i < numberOfDimensions; i++)
                {
                    parameters[i + 1] = itm.Dimensions[i];
                }

                result &= (bool?)method.Invoke(this.metric, parameters) ?? true;
            }

            if (!result)
            {
                logger.LogError(
                    "Could not publish all metrics, limits reached? Series count: {SeriesCount}, Collector count: {CollectorCount}",
                    metric.SeriesCount, values.Count);
            }

            return Task.CompletedTask;
        }
    }
}
