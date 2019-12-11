using System;
using System.Threading.Tasks;

namespace StatsGenerator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var publisher = new RabbitMqPublisher(1_000))
            using (var consumer = new RabbitMqConsumer())
            {
                await Task.WhenAll(publisher.StartAsync(), consumer.StartAsync());

                Console.WriteLine("Press Enter to stop");
                Console.ReadLine();

                await Task.WhenAll(publisher.StopAsync(), consumer.StopAsync());
            }
        }
    }
}
