namespace RabbitMQAzureMetrics.MetricsValueConverters
{
    public class MetricValueItem
    {
        public MetricValueItem(double value, string[] dimensions)
        {
            Value = value;
            Dimensions = dimensions;
        }

        public string[] Dimensions { get; }
        public double Value { get; }
    }
}
