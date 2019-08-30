using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQAzureMetrics.ValuePublishers;
using RabbitMQAzureMetrics.ValuePublishers.Overview;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics
{
    public class RabbitMQMetricsProcessor
    {
        private const int MinPollingInterval = 5_000;
        private readonly HttpClient httpClient;
        private CancellationTokenSource ctsRunning;
        private Task runningTask;
        private readonly Uri baseUri;
        private Dictionary<RabbitMqMetricsEndpoints, Uri> endPointUris;
        private Dictionary<RabbitMqMetricsEndpoints, IValuePublisher> valuePublishers;
        private readonly TelemetryClient client;
        private readonly RabbitMetricsConfiguration configuration;
        private readonly ILogger logger;
        private readonly IApplicationLifetime appLifetime;
        private DateTime lastFlush;
        private static readonly RabbitMqMetricsEndpoints[] allEndpoints = (RabbitMqMetricsEndpoints[])Enum.GetValues(typeof(RabbitMqMetricsEndpoints));

        enum RabbitMqMetricsEndpoints
        {
            Overview = 0,
            Queue,
            Exchange
        }

        public RabbitMQMetricsProcessor(TelemetryClient client, 
            RabbitMetricsConfiguration configuration, 
            ILogger logger,
            IApplicationLifetime appLifetime = null)
        {
            this.httpClient = new HttpClient(new HttpClientHandler { Credentials = new NetworkCredential(configuration.UserName, configuration.Password) });
            var scheme = configuration.UseSSL ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            this.baseUri = new Uri($"{scheme}://{configuration.Hostname}:{configuration.Port}");
            this.client = client;
            this.configuration = configuration;
            this.logger = logger;
            this.appLifetime = appLifetime;

            RegisterEndpointAndPublishers();
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
            int httpRequestExceptionCount = 0;
            int maxRequestExceptionCount = 20;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    for (var i = 0; i < allEndpoints.Length; i++)
                    {
                        var currentEndPoint = allEndpoints[i];

                        var info = await httpClient.GetStringAsync(endPointUris[currentEndPoint]);
                        valuePublishers[currentEndPoint].Publish(info);
                    }

                    FlushIfRequired();

                    httpRequestExceptionCount = 0;
                    await Task.Delay(Math.Max(MinPollingInterval, this.configuration.PollingInterval), cancellationToken);
                }
                catch (HttpRequestException ex)
                {
                    this.logger.LogError("Failed to connect to RabbitMQ at {hostname}. Error: {error}", this.baseUri.AbsoluteUri, ex.Message);
                    if (httpRequestExceptionCount++ < maxRequestExceptionCount)
                    {
                        // todo: add exponential backoff
                        await Task.Delay(15_000);
                    }
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

        private void RegisterEndpointAndPublishers()
        {
            endPointUris = new Dictionary<RabbitMqMetricsEndpoints, Uri>(allEndpoints.Length);
            valuePublishers = new Dictionary<RabbitMqMetricsEndpoints, IValuePublisher>(allEndpoints.Length);

            endPointUris.Add(RabbitMqMetricsEndpoints.Overview, new Uri(baseUri, "api/overview"));
            endPointUris.Add(RabbitMqMetricsEndpoints.Queue, new Uri(baseUri, "api/queues"));
            endPointUris.Add(RabbitMqMetricsEndpoints.Exchange, new Uri(baseUri, "api/exchanges"));

            valuePublishers.Add(RabbitMqMetricsEndpoints.Overview, new MessageOverviewMetricsPublisher(client));
            valuePublishers.Add(RabbitMqMetricsEndpoints.Queue, new QueueMetricsPublisher(client));
            valuePublishers.Add(RabbitMqMetricsEndpoints.Exchange, new ExchangeMetricsPublisher(client));
        }
    }
}
