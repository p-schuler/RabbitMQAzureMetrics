using Newtonsoft.Json.Linq;
using RabbitMQAzureMetrics.Extensions;
using RabbitMQAzureMetrics.MetricsValueConverters;
using System.Collections.Generic;

namespace RabbitMQAzureMetrics.ValuePublishers.Overview
{
    public class ExchangeValueConverter : IMetricsValueConverter
    {
        private const string MessageStats = "message_stats";
        private const string DetailsRateSuffix = "_details.rate";

        private static List<string> publishedMetrics;

        public static IList<string> PublishedMetrics 
        {
            get
            {
                if (publishedMetrics == null)
                {
                    publishedMetrics = new List<string>(DimensionTranslations);
                    publishedMetrics.AddRange(DimensionRateTranslations);
                }
                return publishedMetrics;
            }
        }

        private readonly static string[] PathsWithDetailRate = new[]
        {
            MessageStats + ".publish_in",
            MessageStats + ".publish_out"
        };

        private readonly static string[] DimensionTranslations = new[]
        {
            "Messages published in", //MessageStats + ".publish_in",
            "Messages published out", //MessageStats + ".publish_out"
        };

        private readonly static string[] DimensionRateTranslations = new[]
        {
            "Rate: Messages published in", //MessageStats + ".publish_in",
            "Rate: Messages published out", //MessageStats + ".publish_out"
        };

        public MetricValueCollectionWrapper Convert(string info)
        {
            var exchanges = JArray.Parse(info);

            var collection = new MetricValueCollectionWrapper();

            foreach (var q in exchanges)
            {
                if (q.SelectToken(MessageStats) == null)
                {
                    continue;
                }

                var exchangeName = q.Value<string>("name");
                if (string.IsNullOrEmpty(exchangeName))
                {
                    continue;
                }

                for (var i = 0; i < PathsWithDetailRate.Length; i++)
                {
                    var pathValue = PathsWithDetailRate[i];
                    collection.Add(q.ValueFromPath<float?>($"{pathValue}"), DimensionTranslations[i], exchangeName);
                    collection.Add(q.ValueFromPath<float?>($"{pathValue}{DetailsRateSuffix}"), DimensionRateTranslations[i], exchangeName);
                }
            }

            return collection;
        }
    }
}
