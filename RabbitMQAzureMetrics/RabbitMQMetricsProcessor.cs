using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQAzureMetrics.Consumer;
using RabbitMQAzureMetrics.Processors;
using RabbitMQAzureMetrics.ValuePublishers.AppInsight;
using RabbitMQAzureMetrics.ValuePublishers.Overview;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics
{
    public class RabbitMQMetricsProcessor
    {
        private const int MinPollingInterval = 5_000;
        private CancellationTokenSource ctsRunning;
        private Task runningTask;
        private IList<IMetricProcessor> processors;
        private readonly TelemetryClient client;
        private readonly RabbitMetricsConfiguration configuration;
        private readonly ILogger logger;
        private readonly IHostApplicationLifetime appLifetime;
        private DateTime lastFlush;

        enum RabbitMqMetricsEndpoints
        {
            Overview = 0,
            Queue,
            Exchange
        }

        public RabbitMQMetricsProcessor(TelemetryClient client,
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

            processors = CreateProcessors(configuration, client, httpClientFactory, metricsConsumerLogger);

            logger.LogInformation("Created {0} processors", processors.Count);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (ctsRunning != null)
            {
                throw new InvalidOperationException("Already running");
            }

            ctsRunning = new CancellationTokenSource();
            
            runningTask = Task.Run(async () => 
            {
                using (var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(ctsRunning.Token, cancellationToken))
                {
                    await RunnerAsync(combinedToken.Token);
                }
            });

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            var runningToken = Interlocked.Exchange(ref ctsRunning, null);
            if (runningToken == null)
            {
                throw new InvalidOperationException("Not running");
            }

            runningToken.Cancel();
            await runningTask;

            this.client.Flush();
            runningTask = null;
        }

        private async Task RunnerAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Metrics processor is running");
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    for (var i = 0; i < processors.Count; i++)
                    {
                        await processors[i].ProcessAsync();
                    }

                    FlushIfRequired();

                    await Task.Delay(Math.Max(MinPollingInterval, this.configuration.PollingInterval), cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    this.logger.LogDebug("stopping the metrics processor");
                    break;
                }
                catch (Exception ex)
                {
                    this.logger.LogError("Unexpected error: {error}", ex.Message);

                    Environment.ExitCode = -1;
                    this.appLifetime.StopApplication();
                    break;
                }
            }
        }

        private void FlushIfRequired()
        {
            var now = DateTime.UtcNow;

            // if we have the same configuration for flush delay and the polling
            // we always flush, as we only flush after we did poll the values
            if (this.configuration.FlushDelay != this.configuration.PollingInterval 
            && (now - lastFlush).TotalMilliseconds < this.configuration.FlushDelay)
            {
                return;
            }

            lastFlush = now;
            client.Flush();
        }

        private static IList<IMetricProcessor> CreateProcessors(RabbitMetricsConfiguration configuration, TelemetryClient client, IHttpClientFactory httpClientFactory, ILogger<RabbitMqMetricsConsumer> logger)
        {
            var queueProcessor = new DefaultMetricProcessor(
                        new RabbitMqMetricsConsumer(RabbitMqMetricsConsumer.QueuePath,
                                             configuration,
                                             logger,
                                             new QueueValueConverter(),
                                             httpClientFactory),
                        new TelemetryClientPublisher(MetricsDefinitions.CreateQueueMetric(client)));

            var overviewProcessor = new DefaultMetricProcessor(
                        new RabbitMqMetricsConsumer(RabbitMqMetricsConsumer.OverviewPath,
                                             configuration,
                                             logger,
                                             new MessageOverviewValueConverter(),
                                             httpClientFactory),
                        new TelemetryClientPublisher(MetricsDefinitions.CreateOverviewMetric(client)));

            var exchangeProcessor = new DefaultMetricProcessor(
                        new RabbitMqMetricsConsumer(RabbitMqMetricsConsumer.ExchangePath,
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
    }
}
