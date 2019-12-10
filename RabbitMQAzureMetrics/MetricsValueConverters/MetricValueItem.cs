namespace RabbitMQAzureMetrics.MetricsValueConverters
{
    public class MetricValueItem
    {
        public MetricValueItem(double value, string[] dimensions)
        {
            this.Value = value;
            this.Dimensions = dimensions;
        }

        public string[] Dimensions { get; }

        public double Value { get; }
    }
}
