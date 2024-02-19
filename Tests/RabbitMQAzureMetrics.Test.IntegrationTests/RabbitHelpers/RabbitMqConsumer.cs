using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Test.IntegrationTests
{
    public class RabbitMqConsumer
    {
        private IConnection connection;
        private IModel channel;
        private readonly int consumeDelay;
        private ManualResetEvent mreRunning = new ManualResetEvent(true);

        public RabbitMqConsumer(int consumeDelay = 100)
        {
            this.consumeDelay = consumeDelay;
        }

        public void Pause()
        {
            mreRunning.Reset();
        }

        public void Resume()
        {
            mreRunning.Set();
        }

        public async Task StartAsync()
        {
            if (connection != null) throw new InvalidOperationException("already started");

            var connectionInfo = await RabbitMqConnctionFactory.CreateAsync();

            connection = connectionInfo.Item1;
            channel = connectionInfo.Item2;

            channel.ExchangeDeclare(exchange: RabbitMqConstants.ExchangeName, type: ExchangeType.Fanout);

            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName,
                                exchange: RabbitMqConstants.ExchangeName,
                                routingKey: "");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.Span);
                Console.WriteLine(" [x] {0}", message);
                if (consumeDelay > 0)
                {
                    Thread.Sleep(consumeDelay);
                }
            };

            channel.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            var con = Interlocked.Exchange(ref connection, null);
            if (con != null)
            {
                con.Dispose();
            }

            var channel = Interlocked.Exchange(ref this.channel, null);
            if (channel != null)
            {
                channel.Dispose();
            }

            return Task.CompletedTask;
        }
    }
}
