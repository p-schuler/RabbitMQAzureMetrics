using RabbitMQAzureMetrics.MetricsValueConverters;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.ValuePublishers
{
    public interface IMetricPublisher
    {
        Task PublishAsync(MetricValueCollectionWrapper values);
    }
}
