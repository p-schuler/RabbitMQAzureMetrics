using System;
using System.Collections;
using System.Collections.Generic;


namespace RabbitMQAzureMetrics.MetricsValueConverters
{
    public class MetricValueCollectionWrapper
    {
        public IList<MetricValueItem> Values { get; } = new List<MetricValueItem>();

        public void Add(double value, params string[] dimensions)
        {
            Values.Add(new MetricValueItem(value, dimensions));
        }

        public void Add(double? value, params string[] dimensions)
        {
            if (value.HasValue)
            {
                Values.Add(new MetricValueItem(value.Value, dimensions));
            }
        }
    }
}
