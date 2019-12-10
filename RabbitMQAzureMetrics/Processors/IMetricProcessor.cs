using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Processors
{
    public interface IMetricProcessor
    {
        Task ProcessAsync(CancellationToken cancellationToken = default);
    }
}
