namespace RabbitMQAzureMetrics.Consumer
{
    using System;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Polly;
    using Polly.Extensions.Http;
    using RabbitMQAzureMetrics.Extensions;

    public static class ConsumerExtensions
    {
        private const int MaxRetries = 5;
        private const int MinRetrySeconds = 5;

        public static IServiceCollection AddMetricsConsumer(
                                                            this IServiceCollection services,
                                                            string consumerUserName,
                                                            string consumerPassword,
                                                            int maxRetries = MaxRetries,
                                                            int minRetrySeconds = MinRetrySeconds)
        {
            services.AddHttpClient(Constants.HttpClientName)
                .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                .AddPolicyHandler(GetRetryPolicy(maxRetries, minRetrySeconds))
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
                {
                    Credentials = new NetworkCredential(consumerUserName, consumerPassword),
                });

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int maxRetries, int minRetrySeconds)
        {
            var rnd = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(
                                   MaxRetries,
                                   sleepDurationProvider: (retryAttempt, context) =>
                                    TimeSpan.FromSeconds(Math.Pow(minRetrySeconds, retryAttempt)) + TimeSpan.FromSeconds(rnd.Next(0, minRetrySeconds)),
                                   onRetry: (resp, timespan, context) =>
                                   {
                                       context.GetLogger()?.LogWarning(
                                           "Failed to download metrics. Status Code: {statusCode} Reason: {reason}, Exception: {error}. Delaying for {delay}ms, then making retry.",
                                           resp.Result?.StatusCode,
                                           resp.Result?.ReasonPhrase,
                                           resp.Exception?.Message,
                                           timespan.TotalMilliseconds);
                                   });
        }
    }
}
