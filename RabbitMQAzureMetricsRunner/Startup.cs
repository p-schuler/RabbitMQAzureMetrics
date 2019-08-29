using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace RabbitMQAzureMetrics
{
    class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var config = new RabbitMetricsConfiguration();
            this.configuration.Bind(config);

            services.AddRabbitMqMetrics(config);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) { }
    }
}
