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
        public async Task StartAsync()
        {
            await ConnectAsync();

            Model.ExchangeDeclare(exchange: Constants.ExchangeName, type: ExchangeType.Fanout);

            var queueName = Model.QueueDeclare(queue: "Sample Queue").QueueName;
            Model.QueueBind(queue: queueName,
                                exchange: Constants.ExchangeName,
                                routingKey: "");

            var consumer = new EventingBasicConsumer(Model);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
            };

            Model.BasicConsume(queue: queueName,
                                    autoAck: true,
                                    consumer: consumer);

            Console.WriteLine("consuming messages...");
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}