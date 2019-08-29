using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace RabbitMQAzureMetrics
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var webHost = BuildWebHost(args);
            await webHost.RunAsync();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((ctx, builder) => 
                {
                    builder.AddJsonFile("appsettings.json", false);
                    builder.AddJsonFile("appesttings.local.json", true);
                })
                .UseKestrel()
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

    }
}
