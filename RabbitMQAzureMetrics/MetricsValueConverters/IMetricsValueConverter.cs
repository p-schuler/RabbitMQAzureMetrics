namespace RabbitMQAzureMetrics.MetricsValueConverters
{
    public interface IMetricsValueConverter
    {
        MetricValueCollectionWrapper Convert(string info);
    }
}
