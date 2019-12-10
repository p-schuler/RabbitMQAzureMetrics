using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQAzureMetrics.ValuePublishers.AppInsight
{
    static class MetricsDefinitions
    {
        const string MetricsNamespace = "rabbitmq";

        public static Metric CreateOverviewMetric(TelemetryClient telemetryClient)
        {
            return telemetryClient.GetMetric(new MetricIdentifier(MetricsNamespace, "ChurnRates", "type"));
        }

        public static Metric CreateExchangewMetric(TelemetryClient telemetryClient)
        {
            return telemetryClient.GetMetric(new MetricIdentifier(MetricsNamespace, "Exchange", "Type", "Name"));
        }

        public static Metric CreateQueueMetric(TelemetryClient telemetryClient)
        {
            return telemetryClient.GetMetric(new MetricIdentifier(MetricsNamespace, "Queue", "Type", "Name"));
        }
    }
}
