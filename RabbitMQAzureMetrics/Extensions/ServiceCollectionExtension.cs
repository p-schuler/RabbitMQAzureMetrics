using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RabbitMQAzureMetrics
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddRabbitMqMetrics(this IServiceCollection serviceCollection, RabbitMetricsConfiguration configuration)
        {
            configuration.Validate();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddHostedService<RabbitMqBackgroundService>();

            var appInsightsOptions = new ApplicationInsightsServiceOptions
            {
                EnableAdaptiveSampling = false,
                InstrumentationKey = configuration.AppInsightsKey
            };

            serviceCollection.AddApplicationInsightsTelemetry(appInsightsOptions);

            return serviceCollection;
        }
    }
}
