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
    }
}
