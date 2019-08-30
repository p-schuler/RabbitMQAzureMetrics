using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMQAzureMetrics
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddRabbitMqMetrics(this IServiceCollection serviceCollection, RabbitMetricsConfiguration configuration)
        {
            configuration.Validate();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddHostedService<RabbitMqBackgroundService>();

            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            telemetryConfig.InstrumentationKey = configuration.AppInsightsKey;
            var telemetryClient = new TelemetryClient(telemetryConfig);
            serviceCollection.AddSingleton(telemetryClient);
            return serviceCollection;
        }
    }
}
