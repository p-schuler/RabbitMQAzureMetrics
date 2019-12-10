namespace RabbitMQAzureMetrics.Consumer
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Polly;
    using RabbitMQAzureMetrics.Configuration;
    using RabbitMQAzureMetrics.Extensions;
    using RabbitMQAzureMetrics.MetricsValueConverters;

    public class RabbitMqMetricsConsumer : IMetricConsumer
    {
        public const string QueuePath = "api/queues";
        public const string ExchangePath = "api/exchanges";
        public const string OverviewPath = "api/overview";

        private readonly Uri uri;
        private readonly ILogger<RabbitMqMetricsConsumer> logger;
        private readonly IMetricsValueConverter valueConverter;
        private readonly IHttpClientFactory clientFactory;

        public RabbitMqMetricsConsumer(
                                        string path,
                                        RabbitMetricsConfiguration configuration,
                                        ILogger<RabbitMqMetricsConsumer> logger,
                                        IMetricsValueConverter valueConverter,
                                        IHttpClientFactory clientFactory)
        {
            var scheme = configuration.UseSSL ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            var baseUri = new Uri($"{scheme}://{configuration.RabbitMqHost}:{configuration.RabbitMqPort}");
            this.uri = new Uri(baseUri, path);
            this.logger = logger;
            this.valueConverter = valueConverter;
            this.clientFactory = clientFactory;
        }

        public async Task<MetricValueCollectionWrapper> ConsumeAsync(CancellationToken cancellationToken = default)
        {
            var httpClient = this.clientFactory.CreateClient(Constants.HttpClientName);
            var req = new HttpRequestMessage(HttpMethod.Get, this.uri);
            var context = new Context()
                                .WithLogger(this.logger);
            req.SetPolicyExecutionContext(context);

            this.logger.LogDebug("Trying to fetch metrics from {uri}", this.uri);

            using (var response = await httpClient.SendAsync(req, cancellationToken))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(response.ReasonPhrase);
                }

                var info = await response.Content.ReadAsStringAsync();
                return this.valueConverter.Convert(info);
            }
        }
    }
}
