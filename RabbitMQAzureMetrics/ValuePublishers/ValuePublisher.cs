using Newtonsoft.Json.Linq;

namespace RabbitMQAzureMetrics.ValuePublishers
{
    public abstract class ValuePublisher : IValuePublisher
    {
        protected const string MetricsNamespace = "rabbitmq";
        public abstract void Publish(string info);

        protected T ValueFromPath<T>(JToken jToken, string path)
        {
            var res = jToken.SelectToken(path);
            if (res == null) return default(T);
            return res.Value<T>();
        }
    }
}
