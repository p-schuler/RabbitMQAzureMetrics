namespace RabbitMQAzureMetrics.Processors
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IMetricProcessor
    {
        Task ProcessAsync(CancellationToken cancellationToken = default);
    }
}
