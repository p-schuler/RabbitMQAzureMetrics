using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQAzureMetrics.Consumer;

namespace RabbitMQAzureMetrics
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddRabbitMqMetrics(this IServiceCollection services, RabbitMetricsConfiguration configuration)
        {
            configuration.Validate();
            services.AddSingleton(configuration);
            services.AddHostedService<RabbitMqBackgroundService>();

            var telemetryConfig = TelemetryConfiguration.CreateDefault();
            telemetryConfig.InstrumentationKey = configuration.AppInsightsKey;
            var telemetryClient = new TelemetryClient(telemetryConfig);
            services.AddSingleton(telemetryClient);
            services.AddMetricsConsumer(configuration.RabbitMqUserName, configuration.RabbitMqPassword);
            return services;
        }
    }
}
