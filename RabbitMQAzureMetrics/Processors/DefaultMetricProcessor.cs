namespace RabbitMQAzureMetrics.Processors
{
    using System.Threading;
    using System.Threading.Tasks;
    using RabbitMQAzureMetrics.Consumer;
    using RabbitMQAzureMetrics.ValuePublishers;

    public class DefaultMetricProcessor : IMetricProcessor
    {
        private readonly IMetricConsumer consumer;
        private readonly IMetricPublisher publisher;

        public DefaultMetricProcessor(IMetricConsumer consumer, IMetricPublisher publisher)
        {
            this.consumer = consumer;
            this.publisher = publisher;
        }

        public async Task ProcessAsync(CancellationToken cancellationToken = default)
        {
            var values = await this.consumer.ConsumeAsync(cancellationToken);
            await this.publisher.PublishAsync(values);
        }
    }
}
