using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StatsGenerator
{
    public class RabbitMqConsumer : RabbitMqClient
    {
        private Task consumerTask;
        private CancellationTokenSource consumerCancellationToken;

        public async Task StartAsync()
        {
            await ConnectAsync();

            Model.ExchangeDeclare(exchange: Constants.ExchangeName, type: ExchangeType.Fanout);

            SetupEventBasicConsumerAndQueue("Fast Queue", 10);
            SetupBasicConsumerAndStart("Slow Queue", 2000);

            Console.WriteLine("consuming messages...");
        }

        private void SetupBasicConsumerAndStart(string queueName, int consumeMaxDelay)
        {
            Model.QueueDeclare(queue: queueName);
            Model.QueueBind(queue: queueName,
                                exchange: Constants.ExchangeName,
                                routingKey: "");

            this.consumerCancellationToken = new CancellationTokenSource();
            this.consumerTask = Task.Run(async () => 
            {
                var rnd = new Random();
                while (!this.consumerCancellationToken.IsCancellationRequested)
                {
                    var msg = this.Model.BasicGet(queueName, false);
                    var delay = rnd.Next(0, consumeMaxDelay);
                    try
                    {
                        await Task.Delay(delay, this.consumerCancellationToken.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }

                    if (msg != null)
                    {
                        this.Model.BasicAck(msg.DeliveryTag, false);
                    }
                }
            });
        }

        private void SetupEventBasicConsumerAndQueue(string queueName, int consumeMaxDelay)
        {
            Model.QueueDeclare(queue: queueName);
            Model.QueueBind(queue: queueName,
                                exchange: Constants.ExchangeName,
                                routingKey: "");

            var rnd = new Random();
            var consumer = new EventingBasicConsumer(Model);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.Span);
                lock (rnd)
                {
                    Thread.Sleep(rnd.Next(0, consumeMaxDelay));
                }
            };
            Model.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            this.consumerCancellationToken?.Cancel();
            await this.consumerTask;
        }
    }
}
