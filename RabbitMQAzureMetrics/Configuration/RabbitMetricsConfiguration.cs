namespace RabbitMQAzureMetrics.Configuration
{
    using System;

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
            if (string.IsNullOrWhiteSpace(this.RabbitMqUserName))
            {
                throw new ArgumentException("Missing configuration", nameof(this.RabbitMqUserName));
            }

            if (string.IsNullOrWhiteSpace(this.RabbitMqPassword))
            {
                throw new ArgumentException("Missing configuration", nameof(this.RabbitMqPassword));
            }

            if (string.IsNullOrWhiteSpace(this.RabbitMqHost))
            {
                throw new ArgumentException("Missing configuration", nameof(this.RabbitMqHost));
            }

            if (string.IsNullOrWhiteSpace(this.AppInsightsKey))
            {
                throw new ArgumentException("Missing configuration", nameof(this.AppInsightsKey));
            }
        }
    }
}
