using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Metrics;
using Newtonsoft.Json.Linq;
using RabbitMQAzureMetrics.Extensions;
using System;

namespace RabbitMQAzureMetrics.ValuePublishers.Overview
{
    public class ExchangeMetricsPublisher : ValuePublisher
    {
        private readonly TelemetryClient client;

        private Metric queueStats;

        private const string MessageStats = "message_stats";
        private const string DetailsRateSuffix = "_details.rate";

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

        public ExchangeMetricsPublisher(TelemetryClient client)
        {
            this.client = client;
            InitializeMetrics();
        }

        private void InitializeMetrics()
        {
            queueStats = client.GetMetric(new MetricIdentifier(MetricsNamespace, "Exchange", "Type", "Name"));
        }

        public override void Publish(string info)
        {
            var queues = JArray.Parse(info);

            foreach (var q in queues)
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
                    queueStats.TrackValue(q.ValueFromPath<int>($"{pathValue}"), DimensionTranslations[i], exchangeName);
                    queueStats.TrackValue(q.ValueFromPath<int>($"{pathValue}{DetailsRateSuffix}"), DimensionRateTranslations[i], exchangeName);
                }
            }
        }
    }
}
