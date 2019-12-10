using RabbitMQAzureMetrics.MetricsValueConverters;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Consumer
{
    public interface IMetricConsumer
    {
        Task<MetricValueCollectionWrapper> ConsumeAsync(CancellationToken cancellationToken = default);
    }
}
