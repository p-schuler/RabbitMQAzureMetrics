using RabbitMQAzureMetrics.MetricsValueConverters;
using RabbitMQAzureMetrics.ValuePublishers;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Test.IntegrationTests
{
    public delegate void MetricsPublishedHandler(MetricValueCollectionWrapper collection);

    internal class TestPublisher : IMetricPublisher
    {
        public event MetricsPublishedHandler MetricsPublished;

        public Task PublishAsync(MetricValueCollectionWrapper values)
        {
            MetricsPublished?.Invoke(values);
            return Task.CompletedTask;
        }
    }
}
