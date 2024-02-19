namespace RabbitMQAzureMetrics
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using RabbitMQAzureMetrics.Configuration;
    using RabbitMQAzureMetrics.Consumer;
    using RabbitMQAzureMetrics.Processors;
    using RabbitMQAzureMetrics.ValuePublishers.AppInsight;
    using RabbitMQAzureMetrics.ValuePublishers.Overview;

    public class RabbitMQMetricsProcessor
    {
        private const int MinPollingInterval = 5_000;

        private readonly TelemetryClient client;
        private readonly RabbitMetricsConfiguration configuration;
        private readonly ILogger logger;
        private readonly IHostApplicationLifetime appLifetime;

        private CancellationTokenSource ctsRunning;
        private Task runningTask;
        private IList<IMetricProcessor> processors;
        private DateTime lastFlush;

        public RabbitMQMetricsProcessor(
                                        TelemetryClient client,
                                        RabbitMetricsConfiguration configuration,
                                        ILogger logger,
                                        IHttpClientFactory httpClientFactory,
                                        ILogger<RabbitMqMetricsConsumer> metricsConsumerLogger,
                                        IHostApplicationLifetime appLifetime = null)
        {
            this.client = client;
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;

            this.processors = CreateProcessors(configuration, client, httpClientFactory, metricsConsumerLogger);

            logger.LogInformation("Created {0} processors", this.processors.Count);
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await RunnerAsync(cancellationToken);
        }

        private static IList<IMetricProcessor> CreateProcessors(RabbitMetricsConfiguration configuration, TelemetryClient client, IHttpClientFactory httpClientFactory, ILogger<RabbitMqMetricsConsumer> logger)
        {
            var queueProcessor = new DefaultMetricProcessor(
                        new RabbitMqMetricsConsumer(
                                             RabbitMqMetricsConsumer.QueuePath,
                                             configuration,
                                             logger,
                                             new QueueValueConverter(),
                                             httpClientFactory),
                        new TelemetryClientPublisher(MetricsDefinitions.CreateQueueMetric(client)));

            var overviewProcessor = new DefaultMetricProcessor(
                        new RabbitMqMetricsConsumer(
                                             RabbitMqMetricsConsumer.OverviewPath,
                                             configuration,
                                             logger,
                                             new MessageOverviewValueConverter(),
                                             httpClientFactory),
                        new TelemetryClientPublisher(MetricsDefinitions.CreateOverviewMetric(client)));

            var exchangeProcessor = new DefaultMetricProcessor(
                        new RabbitMqMetricsConsumer(
                                             RabbitMqMetricsConsumer.ExchangePath,
                                             configuration,
                                             logger,
                                             new ExchangeValueConverter(),
                                             httpClientFactory),
                        new TelemetryClientPublisher(MetricsDefinitions.CreateExchangewMetric(client)));

            var processors = new List<IMetricProcessor>(3);

            processors.Add(queueProcessor);
            processors.Add(overviewProcessor);
            processors.Add(exchangeProcessor);

            return processors;
        }

        private async Task RunnerAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("Metrics processor is running");
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var t in this.processors)
                {
                    await t.ProcessAsync(cancellationToken);
                }

                this.FlushIfRequired();

                await Task.Delay(Math.Max(MinPollingInterval, this.configuration.PollingInterval), cancellationToken);
            }
        }

        private void FlushIfRequired()
        {
            var now = DateTime.UtcNow;

            // if we have the same configuration for flush delay and the polling
            // we always flush, as we only flush after we did poll the values
            if (this.configuration.FlushDelay != this.configuration.PollingInterval
            && (now - this.lastFlush).TotalMilliseconds < this.configuration.FlushDelay)
            {
                return;
            }

            this.lastFlush = now;
            this.client.Flush();
        }
    }
}
