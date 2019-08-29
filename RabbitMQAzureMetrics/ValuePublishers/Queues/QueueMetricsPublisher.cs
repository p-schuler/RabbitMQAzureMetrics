using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;
using Newtonsoft.Json.Linq;
using RabbitMQAzureMetrics.Extensions;

namespace RabbitMQAzureMetrics.ValuePublishers.Overview
{
    public class QueueMetricsPublisher : ValuePublisher
    {
        private readonly TelemetryClient client;

        private Metric queueStats;

        private const string MessageStats = "message_stats";
        private const string DetailsRateSuffix = "_details.rate";

        private readonly static string[] PathsWithDetailRate = new[]
        {
            "messages",
            "messages_ready",
            "messages_unacknowledged",
            MessageStats + ".ack",
            MessageStats + ".deliver_get",
            MessageStats + ".deliver_no_ack",
            MessageStats + ".get",
            MessageStats + ".get_no_ack",
            MessageStats + ".publish",
            MessageStats + ".redeliver"
        };

        public QueueMetricsPublisher(TelemetryClient client)
        {
            this.client = client;
            InitializeMetrics();
        }

        private void InitializeMetrics()
        {
            queueStats = client.GetMetric(new MetricIdentifier(MetricsNamespace, "Queue", "Type", "Name"));
        }

        public override void Publish(string info)
        {
            var queues = JArray.Parse(info);

            foreach (var q in queues)
            {
                var qName = q.Value<string>("name");
                for (var i = 0; i < PathsWithDetailRate.Length; i++)
                {
                    var pathValue = PathsWithDetailRate[i];
                    queueStats.TrackValue(q.ValueFromPath<int>($"{pathValue}"), pathValue, qName);
                    queueStats.TrackValue(q.ValueFromPath<int>($"{pathValue}{DetailsRateSuffix}"), pathValue + DetailsRateSuffix, qName);
                }
            }
        }
    }
}
