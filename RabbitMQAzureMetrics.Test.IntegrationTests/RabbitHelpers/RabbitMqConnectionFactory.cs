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
    public static class RabbitMqConnctionFactory
    {
        public static async Task<Tuple<IConnection, IModel>> CreateAsync()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var policy = Policy
              .Handle<BrokerUnreachableException>()
              .WaitAndRetryAsync(30, retryAttempt => TimeSpan.FromSeconds(2));

            IConnection connection = null;
            IModel model = null;

            await policy.ExecuteAsync(() =>
            {
                connection = factory.CreateConnection();
                model = connection.CreateModel();
                return Task.CompletedTask;
            });

            return new Tuple<IConnection, IModel>(connection, model);
        }
    }
}