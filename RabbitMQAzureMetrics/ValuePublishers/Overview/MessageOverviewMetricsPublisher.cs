using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;
using Newtonsoft.Json.Linq;
using RabbitMQAzureMetrics.Extensions;

namespace RabbitMQAzureMetrics.ValuePublishers.Overview
{
    public class MessageOverviewMetricsPublisher : ValuePublisher
    {
        private readonly TelemetryClient client;

        private Metric churnRates;

        private readonly static string[] ChurnRatesPaths = new[]
        {
            "channel_closed",
            "channel_created",
            "connection_closed",
            "connection_created",
            "queue_created",
            "queue_declared",
            "queue_deleted"
        };

        private readonly static string[] DimensionTranslations = new[]
        {
            "Total channels closed",// "channel_closed",
            "Total channels created", // "channel_created",
            "Total connections closed", // "connection_closed",
            "Total connections created", // "connection_created",
            "Total queues created", // "queue_created",
            "Total queues declared", // "queue_declared",
            "Total queues deleted", // "queue_deleted"
        };

        private const string ChurnRates = "churn_rates";
        
        public MessageOverviewMetricsPublisher(TelemetryClient client)
        {
            this.client = client;
            InitializeMetrics();
        }

        private void InitializeMetrics()
        {
            churnRates = client.GetMetric(new MetricIdentifier(MetricsNamespace, ChurnRates, "type"));
        }

        public override void Publish(string info)
        {
            var overviewInfo = JObject.Parse(info);

            for (var i = 0; i < ChurnRatesPaths.Length; i++)
            {
                var path = ChurnRatesPaths[i];
                churnRates.TrackValue(overviewInfo.ValueFromPath<int>($"{ChurnRates}.{path}"), DimensionTranslations[i]);
            }
        }
    }
}
