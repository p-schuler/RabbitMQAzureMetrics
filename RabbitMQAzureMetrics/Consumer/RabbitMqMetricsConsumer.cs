using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQAzureMetrics.Extensions;
using RabbitMQAzureMetrics.MetricsValueConverters;

namespace RabbitMQAzureMetrics.Consumer
{
    public class RabbitMqMetricsConsumer : IMetricConsumer
    {
        private readonly Uri uri;
        private readonly ILogger<RabbitMqMetricsConsumer> logger;
        private readonly IMetricsValueConverter valueConverter;
        private readonly IHttpClientFactory clientFactory;
        
        public const string QueuePath = "api/queues";
        public const string ExchangePath = "api/exchanges";
        public const string OverviewPath = "api/overview";

        public RabbitMqMetricsConsumer(string path, 
                                RabbitMetricsConfiguration configuration, 
                                ILogger<RabbitMqMetricsConsumer> logger,
                                IMetricsValueConverter valueConverter,
                                IHttpClientFactory clientFactory)
        {
            var scheme = configuration.UseSSL ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
            var baseUri = new Uri($"{scheme}://{configuration.RabbitMqHost}:{configuration.RabbitMqPort}");
            uri = new Uri(baseUri, path);
            this.logger = logger;
            this.valueConverter = valueConverter;
            this.clientFactory = clientFactory;
        }

        public async Task<MetricValueCollectionWrapper> ConsumeAsync(CancellationToken cancellationToken = default)
        {
            var httpClient = clientFactory.CreateClient(Constants.HttpClientName);
            var req = new HttpRequestMessage(HttpMethod.Get, uri);
            var context = new Context()
                                .WithLogger(logger);
            req.SetPolicyExecutionContext(context);

            logger.LogDebug("Trying to fetch metrics from {uri}", uri);

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
