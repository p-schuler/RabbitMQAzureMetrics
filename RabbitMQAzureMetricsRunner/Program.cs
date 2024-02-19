using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQAzureMetrics;
using RabbitMQAzureMetrics.Configuration;

using var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        var config = new RabbitMetricsConfiguration();
        hostContext.Configuration.Bind(config);
        services.AddRabbitMqMetrics(config);
    })
    .Build();

await host.StartAsync();
await host.WaitForShutdownAsync();
