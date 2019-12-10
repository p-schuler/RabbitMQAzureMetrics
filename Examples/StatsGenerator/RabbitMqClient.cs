using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StatsGenerator
{
    public class RabbitMqClient : IDisposable
    {
        protected IConnection Connection { get; private set; }
        protected IModel Model { get; private set; }

        protected async Task ConnectAsync(string hostname = "localhost", string userName = "guest", string password = "guest", CancellationToken cancellationToken = default)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var policy = Policy
              .Handle<BrokerUnreachableException>()
              .WaitAndRetryAsync(30, retryAttempt => TimeSpan.FromSeconds(2));

            await policy.ExecuteAsync((cts) =>
            {
                Connection = factory.CreateConnection();
                Model = Connection.CreateModel();
                return Task.CompletedTask;
            }, cancellationToken);
        }

        public void Dispose()
        {
            Connection?.Dispose();
            Connection = null;

            Model?.Dispose();
            Model = null;
        }
    }
}