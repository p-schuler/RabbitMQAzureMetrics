using System;

namespace RabbitMQAzureMetrics
{
    public class RabbitMetricsConfiguration
    {
        public string RabbitMqUserName { get; set; } = "guest";
        public string RabbitMqPassword { get; set; } = "guest";
        public int RabbitMqPort { get; set; } = 15672;
        public string RabbitMqHost { get; set; } = "localhost";
        public bool UseSSL { get; set; }
        public int PollingInterval { get; set; } = 5_000;
        public int FlushDelay { get; set; } = 30_000;
        public string AppInsightsKey { get; set; }

        internal void Validate()
        {
            if (String.IsNullOrWhiteSpace(RabbitMqUserName)) throw new ArgumentException("Missing configuration", nameof(RabbitMqUserName));
            if (String.IsNullOrWhiteSpace(RabbitMqPassword)) throw new ArgumentException("Missing configuration", nameof(RabbitMqPassword));
            if (String.IsNullOrWhiteSpace(RabbitMqHost)) throw new ArgumentException("Missing configuration", nameof(RabbitMqHost));
            if (String.IsNullOrWhiteSpace(AppInsightsKey)) throw new ArgumentException("Missing configuration", nameof(AppInsightsKey));
        }
    }
}
