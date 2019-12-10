using Microsoft.ApplicationInsights;
using RabbitMQAzureMetrics.Consumer;
using RabbitMQAzureMetrics.ValuePublishers;
using RabbitMQAzureMetrics.ValuePublishers.AppInsight;
using RabbitMQAzureMetrics.ValuePublishers.Overview;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Processors
{
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
            var values = await consumer.ConsumeAsync(cancellationToken);
            await publisher.PublishAsync(values);
        }
    }
}
