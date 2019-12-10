namespace RabbitMQAzureMetrics.ValuePublishers
{
    using System.Threading.Tasks;
    using RabbitMQAzureMetrics.MetricsValueConverters;

    public interface IMetricPublisher
    {
        Task PublishAsync(MetricValueCollectionWrapper values);
    }
}
