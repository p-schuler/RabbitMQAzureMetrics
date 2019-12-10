namespace RabbitMQAzureMetrics.ValuePublishers.Overview
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using RabbitMQAzureMetrics.Extensions;
    using RabbitMQAzureMetrics.MetricsValueConverters;

    public class QueueValueConverter : IMetricsValueConverter
    {
        private const string MessageStats = "message_stats";
        private const string DetailsRateSuffix = "_details.rate";

        private static readonly string[] Paths = new[]
        {
            "consumers",
            "consumer_utilisation",
        };

        private static readonly string[] PathsDimensionTranslations = new[]
        {
            "Consumer: Count",
            "Consumer: Utilisation",
        };

        private static readonly string[] PathsWithDetailRate = new[]
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
            MessageStats + ".redeliver",
        };

        private static readonly string[] StatsDimensionTranslations = new[]
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

        private static readonly string[] StatsDimensionRateTranslations = new[]
        {
            "Rate: Total number of messages", // "messages",
            "Rate: Messages ready for delivery", // "messages_ready",
            "Rate: Number of unacknowledged messages", // "messages_unacknowledged",
            "Rate: Total number of messages in ack mode", // MessageStats + ".ack",
            "Rate: Messages delivered recently (of all modes)", // MessageStats + ".deliver_get",
            "Rate: Messages delivered in no-ack mode to consumers.", // MessageStats + ".deliver_no_ack",
            "Rate: Messages delivered in ack mode in response to basic.get", // MessageStats + ".get",
            "Rate: Messages delivered in no-ack mode in response to basic.get", // MessageStats + ".get_no_ack",
            "Rate: Message publishing", // MessageStats + ".publish",
            "Rate: Count of subset of messages in deliver_get which had the redelivered flag set.", // MessageStats + ".redeliver"
        };

        private static readonly string[] CalculationTranslations = new[]
        {
            "Rate: delivery delay",
        };

        private static readonly Action<JToken, MetricValueCollectionWrapper, string>[] Calculations = new Action<JToken, MetricValueCollectionWrapper, string>[]
        {
            CalculateDeliveryDelay,
        };

        private static List<string> publishedMetrics;

        private enum CalculationType
        {
            DeliveryDelay = 0,
        }

        public static IList<string> PublishedMetrics
        {
            get
            {
                if (publishedMetrics == null)
                {
                    publishedMetrics = new List<string>(StatsDimensionRateTranslations);
                    publishedMetrics.AddRange(StatsDimensionTranslations);
                    publishedMetrics.AddRange(PathsDimensionTranslations);
                    publishedMetrics.AddRange(CalculationTranslations);
                }

                return publishedMetrics;
            }
        }

        public MetricValueCollectionWrapper Convert(string info)
        {
            var collection = new MetricValueCollectionWrapper();
            var queues = JArray.Parse(info);

            foreach (var q in queues)
            {
                var qName = q.Value<string>("name");

                for (var i = 0; i < PathsWithDetailRate.Length; i++)
                {
                    var pathValue = PathsWithDetailRate[i];
                    collection.Add(q.ValueFromPath<float?>($"{pathValue}"), StatsDimensionTranslations[i], qName);
                    collection.Add(q.ValueFromPath<float?>($"{pathValue}{DetailsRateSuffix}"), StatsDimensionRateTranslations[i], qName);
                }

                for (var i = 0; i < Paths.Length; i++)
                {
                    var pathValue = Paths[i];
                    collection.Add(q.ValueFromPath<float?>($"{pathValue}"), PathsDimensionTranslations[i], qName);
                }

                for (var i = 0; i < Calculations.Length; i++)
                {
                    Calculations[i](q, collection, qName);
                }
            }

            return collection;
        }

        /// <summary>
        /// Calculates the delay of the delivery relative to the publishing of the messages and publishes
        /// that value in the passed in metric.
        /// </summary>
        /// <param name="jToken">Token to use to parse.</param>
        /// <param name="collection">The collection to store the value.</param>
        /// <param name="queueName">The queue name of the metric.</param>
        private static void CalculateDeliveryDelay(JToken jToken, MetricValueCollectionWrapper collection, string queueName)
        {
            var deliverRate = jToken.ValueFromPath<float>(MessageStats + ".deliver_get" + DetailsRateSuffix);
            var publishRate = jToken.ValueFromPath<float>(MessageStats + ".publish" + DetailsRateSuffix);

            var relativeDelay = (publishRate > 0) ? Math.Round(1 - (deliverRate / publishRate), 2) : 0;
            collection.Add((float)relativeDelay, CalculationTranslations[(int)CalculationType.DeliveryDelay], queueName);
        }
    }
}
