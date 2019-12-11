using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatsGenerator
{
    public class RabbitMqPublisher : RabbitMqClient
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly int maxPublisherDelay;
        private Task currentTask;
        public int MessagesPublished { get; }

        public RabbitMqPublisher(int maxPublisherDelay = 500)
        {
            this.maxPublisherDelay = maxPublisherDelay;
        }

        public async Task StartAsync()
        {
            await ConnectAsync();

            this.currentTask = Task.Run(async () => 
            {
                Model.ExchangeDeclare(exchange: Constants.ExchangeName, type: ExchangeType.Fanout);

                var message = "integration test message";
                var body = Encoding.UTF8.GetBytes(message);

                Console.WriteLine("publishing messages...");

                var rnd = new Random();

                while (!this.cts.IsCancellationRequested)
                {
                    Model.BasicPublish(exchange: Constants.ExchangeName,
                                        routingKey: "",
                                        basicProperties: null,
                                        body: body);
                    await Task.Delay(rnd.Next(0, maxPublisherDelay));
                }
            });
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (this.currentTask == null) return;

            this.cts.Cancel();
            await this.currentTask;
        }
    }
}