using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitMQAzureMetrics
{
    public class RabbitMetricsConfiguration
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; } = 15672;
        public string Hostname { get; set; }
        public bool UseSSL { get; set; }
        public int PollingInterval { get; set; } = 5_000;
        public int FlushDelay { get; set; } = 30_000;
        public string AppInsightsKey { get; set; }

        internal void Validate()
        {
            if (String.IsNullOrWhiteSpace(UserName)) throw new ArgumentException("Missing configuration", nameof(UserName));
            if (String.IsNullOrWhiteSpace(Password)) throw new ArgumentException("Missing configuration", nameof(Password));
            if (String.IsNullOrWhiteSpace(Hostname)) throw new ArgumentException("Missing configuration", nameof(Hostname));
            if (String.IsNullOrWhiteSpace(AppInsightsKey)) throw new ArgumentException("Missing configuration", nameof(AppInsightsKey));
        }
    }
}
