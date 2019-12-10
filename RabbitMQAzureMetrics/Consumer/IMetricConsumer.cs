namespace RabbitMQAzureMetrics.Consumer
{
    using System.Threading;
    using System.Threading.Tasks;
    using RabbitMQAzureMetrics.MetricsValueConverters;

    public interface IMetricConsumer
    {
        Task<MetricValueCollectionWrapper> ConsumeAsync(CancellationToken cancellationToken = default);
    }
}
