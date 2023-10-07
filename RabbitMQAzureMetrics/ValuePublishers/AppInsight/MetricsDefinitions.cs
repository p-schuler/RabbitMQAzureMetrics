using RabbitMQAzureMetrics.ValuePublishers.Overview;

namespace RabbitMQAzureMetrics.ValuePublishers.AppInsight
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Metrics;

    internal static class MetricsDefinitions
    {
        private const string MetricsNamespace = "rabbitmq";

        public static Metric CreateOverviewMetric(TelemetryClient telemetryClient)
        {
            return telemetryClient.GetMetric(new MetricIdentifier(MetricsNamespace, "ChurnRates", "type"));
        }

        public static Metric CreateExchangewMetric(TelemetryClient telemetryClient)
        {
            return telemetryClient.GetMetric(new MetricIdentifier(MetricsNamespace, "Exchange", "Type", "Name"));
        }

        public static Metric CreateQueueMetric(TelemetryClient telemetryClient, int queueLimit = 1_000)
        {
            return telemetryClient.GetMetric(new MetricIdentifier(MetricsNamespace, "Queue", "Type", "Name"),
                new MetricConfiguration(QueueValueConverter.PublishedMetrics.Count * queueLimit, queueLimit,
                    new MetricSeriesConfigurationForMeasurement(restrictToUInt32Values: false)));
        }
    }
}
