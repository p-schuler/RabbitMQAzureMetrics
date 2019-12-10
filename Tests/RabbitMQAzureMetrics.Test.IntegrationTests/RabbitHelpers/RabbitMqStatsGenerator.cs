using NUnit.Framework;
using NUnit.Framework.Constraints;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Test.IntegrationTests
{
    public class RabbitMqStatsGenerator
    {
        private RabbitMqPublisher publisher;
        private RabbitMqConsumer consumer;

        public void PauseConsumer()
        {
            var consumer = this.consumer;
            if (consumer != null)
            {
                consumer.Pause();
            }
        }

        public void ResumeConsumer()
        {
            var consumer = this.consumer;
            if (consumer != null)
            {
                consumer.Resume();
            }
        }

        public async Task StartAsync()
        {
            if (publisher != null) return;
            publisher = new RabbitMqPublisher();
            consumer = new RabbitMqConsumer();
            await Task.WhenAll(publisher.StartAsync(), consumer.StartAsync());
        }

        public async Task StopAsync()
        {
            var publisher = Interlocked.Exchange(ref this.publisher, null);
            var consumer = Interlocked.Exchange(ref this.consumer, null);
            await Task.WhenAll(publisher.StopAsync(), consumer.StopAsync());
        }

        public static async Task GenerateStatsAsync(int numberOfSecondsToRun)
        {
            var sender = new RabbitMqPublisher();
            var receiver = new RabbitMqConsumer();
            try
            {
                await sender.StartAsync();
                await receiver.StartAsync();

                await Task.Delay(numberOfSecondsToRun * 1000);
            }
            finally
            {
                await sender.StopAsync();
                await receiver.StopAsync();
            }
        }
    }
}