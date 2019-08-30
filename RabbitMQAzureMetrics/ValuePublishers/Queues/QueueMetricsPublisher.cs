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

        private readonly static string[] DimensionTranslations = new[]
        {
            "Total number of messages",
            "Messages ready for delivery",
            "Number of unacknowledged messages",
            "Total number of messages in ack mode", // MessageStats + ".ack",
            "Messages delivered recently (of all modes)", // deliver_get
            "Messages delivered in no-ack mode to consumers.", // deliver_no_ack
            "Messages delivered in ack mode in response to basic.get", // get
            "Messages delivered in no-ack mode in response to basic.get", // get_no_ack
            "Messages published recently", // publish
            "Count of subset of messages in deliver_get which had the redelivered flag set.", // redliver
        };

        private readonly static string[] DimensionRateTranslations = new []
        {
            "Rate: Total number of messages", //"messages",
            "Rate: Messages ready for delivery", //"messages_ready",
            "Rate: Number of unacknowledged messages", //"messages_unacknowledged",
            "Rate: Total number of messages in ack mode", // MessageStats + ".ack",
            "Rate: Messages delivered recently (of all modes)", //MessageStats + ".deliver_get",
            "Rate: Messages delivered in no-ack mode to consumers.", //MessageStats + ".deliver_no_ack",
            "Rate: Messages delivered in ack mode in response to basic.get", //MessageStats + ".get",
            "Rate: Messages delivered in no-ack mode in response to basic.get", //MessageStats + ".get_no_ack",
            "Rate: Message publishing", //MessageStats + ".publish",
            "Rate: Count of subset of messages in deliver_get which had the redelivered flag set.", //MessageStats + ".redeliver"
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
                    queueStats.TrackValue(q.ValueFromPath<int>($"{pathValue}"), DimensionTranslations[i], qName);
                    queueStats.TrackValue(q.ValueFromPath<int>($"{pathValue}{DetailsRateSuffix}"), DimensionRateTranslations[i], qName);
                }
            }
        }
    }
}
