namespace RabbitMQAzureMetrics
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RabbitMQAzureMetrics.Configuration;
    using RabbitMQAzureMetrics.Consumer;

    internal class RabbitMqBackgroundService : BackgroundService
    {
        private readonly ILogger<RabbitMqBackgroundService> logger;
        private readonly RabbitMQMetricsProcessor processor;

        public RabbitMqBackgroundService(
                                         ILogger<RabbitMqBackgroundService> logger,
                                         IHostApplicationLifetime appLifetime,
                                         TelemetryClient telemetryClient,
                                         RabbitMetricsConfiguration configuration,
                                         IHttpClientFactory clientFactory,
                                         ILogger<RabbitMqMetricsConsumer> metricsConsumerLogger)
        {
            this.logger = logger;
            this.processor = new RabbitMQMetricsProcessor(telemetryClient, configuration, logger, clientFactory, metricsConsumerLogger, appLifetime);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Starting RabbitMQ Metrics Service");
            await this.processor.ExecuteAsync(stoppingToken);
            this.logger.LogInformation("Stopped RabbitMQ Metrics Service");
        }
    }
}
