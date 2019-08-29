using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics
{
    class RabbitMqBackgroundService : IHostedService
    {
        private readonly ILogger<RabbitMqBackgroundService> logger;
        private readonly RabbitMQMetricsProcessor processor;

        public RabbitMqBackgroundService(ILogger<RabbitMqBackgroundService> logger, 
                                         IApplicationLifetime appLifetime, 
                                         TelemetryClient telemetryClient,
                                         RabbitMetricsConfiguration configuration)
        {
            this.logger = logger;
            processor = new RabbitMQMetricsProcessor(telemetryClient, configuration, logger, appLifetime);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Starting RabbitMQ Metrics Service");
            await processor.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Stopping RabbitMQ Metrics Service");
            await processor.StopAsync(cancellationToken);
        }
    }
}
