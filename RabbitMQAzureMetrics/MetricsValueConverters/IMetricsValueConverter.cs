using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQAzureMetrics.MetricsValueConverters
{
    public interface IMetricsValueConverter
    {
        MetricValueCollectionWrapper Convert(string info);
    }
}
