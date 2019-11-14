using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;
using Newtonsoft.Json.Linq;
using RabbitMQAzureMetrics.Extensions;
using System;

namespace RabbitMQAzureMetrics.ValuePublishers.Overview
{
    public class QueueMetricsPublisher : ValuePublisher
    {
        private readonly TelemetryClient client;

        private Metric queueStats;

        private const string MessageStats = "message_stats";
        private const string DetailsRateSuffix = "_details.rate";

        private readonly static string[] FloatPaths = new[]
        {
            "consumer_utilisation"
        };

        private readonly static string[] FloatPathsDimensionTranslations = new[]
        {
            "Consumer: Utilisation"
        };

        private readonly static string[] Paths = new[]
        {
            "consumers"
        };

        private readonly static string[] PathsDimensionTranslations = new[]
        {
            "Consumer: Count"
        };

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

        private readonly static string[] StatsDimensionTranslations = new[]
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

        private readonly static string[] StatsDimensionRateTranslations = new[]
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

        private readonly static Action<JToken, Metric, string>[] Calculations = new Action<JToken, Metric, string>[]
        {
            CalculateDeliveryDelay
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
                    queueStats.TrackValue(q.ValueFromPath<int>($"{pathValue}"), StatsDimensionTranslations[i], qName);
                    queueStats.TrackValue(q.ValueFromPath<float>($"{pathValue}{DetailsRateSuffix}"), StatsDimensionRateTranslations[i], qName);
                }

                for (var i = 0; i < Paths.Length; i++)
                {
                    var pathValue = Paths[i];
                    queueStats.TrackValue(q.ValueFromPath<int>($"{pathValue}"), PathsDimensionTranslations[i], qName);
                }

                for (var i = 0; i < FloatPaths.Length; i++)
                {
                    var pathValue = FloatPaths[i];
                    queueStats.TrackValue(q.ValueFromPath<float>($"{pathValue}"), FloatPathsDimensionTranslations[i], qName);
                }

                for (var i = 0; i < Calculations.Length; i++)
                {
                    Calculations[i](q, queueStats, qName);
                }
            }
        }

        /// <summary>
        /// Calculates the delay of the delivery relative to the publishing of the messages and publishes
        /// that value in the passed in metric.
        /// </summary>
        /// <param name="jToken"></param>
        /// <param name="targetMetric"></param>
        /// <param name="queueName"></param>
        private static void CalculateDeliveryDelay(JToken jToken, Metric targetMetric, string queueName)
        {
            var deliverRate = jToken.ValueFromPath<float>(MessageStats + ".deliver_get" + DetailsRateSuffix);
            var publishRate = jToken.ValueFromPath<float>(MessageStats + ".publish" + DetailsRateSuffix);

            var relativeDelay = (publishRate > 0) ? Math.Round(1 - (deliverRate / publishRate), 2) : 0;
            targetMetric.TrackValue(relativeDelay, "Rate: delivery delay", queueName);
        }
    }
}
