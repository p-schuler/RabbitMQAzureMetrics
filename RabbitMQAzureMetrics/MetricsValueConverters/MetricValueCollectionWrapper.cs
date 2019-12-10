namespace RabbitMQAzureMetrics.MetricsValueConverters
{
    using System.Collections.Generic;

    public class MetricValueCollectionWrapper
    {
        public IList<MetricValueItem> Values { get; } = new List<MetricValueItem>();

        public void Add(double value, params string[] dimensions)
        {
            this.Values.Add(new MetricValueItem(value, dimensions));
        }

        public void Add(double? value, params string[] dimensions)
        {
            if (value.HasValue)
            {
                this.Values.Add(new MetricValueItem(value.Value, dimensions));
            }
        }
    }
}
