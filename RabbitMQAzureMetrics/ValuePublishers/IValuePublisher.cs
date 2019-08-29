namespace RabbitMQAzureMetrics.ValuePublishers
{
    public interface IValuePublisher
    {
        void Publish(string info);
    }
}
