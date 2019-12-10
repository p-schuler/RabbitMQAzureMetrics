using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Test.IntegrationTests
{
    public class RabbitMqPublisher
    {
        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly int publisherDelay;
        private Task currentTask;
        public int MessagesPublished { get; }

        public RabbitMqPublisher(int publisherDelay = 50)
        {
            this.publisherDelay = publisherDelay;
        }

        public async Task StartAsync()
        {
            var tcs = new TaskCompletionSource<object>();

            var connectionInfo = await RabbitMqConnctionFactory.CreateAsync();

            var connection = connectionInfo.Item1;
            var channel = connectionInfo.Item2;

            this.currentTask = Task.Run(async () => 
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                
                using (connection)
                using (channel)
                {
                    channel.ExchangeDeclare(exchange: RabbitMqConstants.ExchangeName, type: ExchangeType.Fanout);

                    var message = "integration test message";
                    var body = Encoding.UTF8.GetBytes(message);

                    tcs.SetResult(null);

                    while (!this.cts.IsCancellationRequested)
                    {
                        channel.BasicPublish(exchange: RabbitMqConstants.ExchangeName,
                                            routingKey: "",
                                            basicProperties: null,
                                            body: body);
                        await Task.Delay(publisherDelay);
                    }
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