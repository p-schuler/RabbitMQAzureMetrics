namespace RabbitMQAzureMetrics.ValuePublishers.Overview
{
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using RabbitMQAzureMetrics.Extensions;
    using RabbitMQAzureMetrics.MetricsValueConverters;

    public class MessageOverviewValueConverter : IMetricsValueConverter
    {
        private const string ChurnRates = "churn_rates";

        private static readonly string[] ChurnRatesPaths = new[]
        {
            "channel_closed",
            "channel_created",
            "connection_closed",
            "connection_created",
            "queue_created",
            "queue_declared",
            "queue_deleted",
        };

        private static readonly string[] DimensionTranslations = new[]
        {
            "Total channels closed", // "channel_closed",
            "Total channels created", // "channel_created",
            "Total connections closed", // "connection_closed",
            "Total connections created", // "connection_created",
            "Total queues created", // "queue_created",
            "Total queues declared", // "queue_declared",
            "Total queues deleted", // "queue_deleted"
        };

        private static List<string> publishedMetrics;

        public static IList<string> PublishedMetrics
        {
            get
            {
                if (publishedMetrics == null)
                {
                    publishedMetrics = new List<string>(DimensionTranslations);
                }

                return publishedMetrics;
            }
        }

        public MetricValueCollectionWrapper Convert(string info)
        {
            var collection = new MetricValueCollectionWrapper();

            var overviewInfo = JObject.Parse(info);

            for (var i = 0; i < ChurnRatesPaths.Length; i++)
            {
                var path = ChurnRatesPaths[i];
                collection.Add(overviewInfo.ValueFromPath<float?>($"{ChurnRates}.{path}"), DimensionTranslations[i]);
            }

            return collection;
        }
    }
}
