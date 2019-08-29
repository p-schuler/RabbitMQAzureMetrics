using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
            var rabbitConfig = new RabbitMetricsConfiguration
            {
                Hostname = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            services.AddRabbitMqMetrics(rabbitConfig, "[your app insights key]");
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) { }
    }
}
