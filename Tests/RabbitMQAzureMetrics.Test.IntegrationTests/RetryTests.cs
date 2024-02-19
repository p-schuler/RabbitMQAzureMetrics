using Castle.Core.Logging;
using debugerr.Test.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using RabbitMQAzureMetrics.Configuration;
using RabbitMQAzureMetrics.Consumer;
using RabbitMQAzureMetrics.Processors;
using RabbitMQAzureMetrics.ValuePublishers.Overview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Test.IntegrationTests
{
    public class RetryTests
    {

        [Test]
        public async Task When_Rabbit_Is_Not_Responding_Collector_Retries_And_Recovers()
        {
            var moqLogger = new Mock<ILogger<RabbitMqMetricsConsumer>>();

            var serviceProvider = InitServiceProvider(5, 2);

            var publisher = new TestPublisher();
            var processor = new DefaultMetricProcessor(new RabbitMqMetricsConsumer(RabbitMqMetricsConsumer.QueuePath,
                                                                                      new RabbitMetricsConfiguration(),
                                                                                      moqLogger.Object,
                                                                                      new QueueValueConverter(),
                                                                                      serviceProvider.GetService<IHttpClientFactory>()),
                                                        publisher);
            var mre = new ManualResetEvent(false);
            publisher.MetricsPublished += (c) =>
            {
                mre.Set();
            };

            await RabbitMqTestFixture.RabbitTestContainer.StopAsync();

            var processTask = processor.ProcessAsync();

            await WaitForCondition.AssertAsync(() => 
                moqLogger.Invocations.FirstOrDefault(x =>
                        x.Method.Name == nameof(Microsoft.Extensions.Logging.ILogger.Log) &&
                        x.Arguments?.FirstOrDefault(x => x != null && x.ToString().StartsWith("Failed to download metrics.")) != null) != null,
                        "retry performed", 
                        pollingInterval: 2000);

            // verify that there was nothing published
            Assert.That(mre.WaitOne(1) == false);

            // start the container
            await RabbitMqTestFixture.RabbitTestContainer.StartAsync();

            // now the processor should recover
            Assert.That(mre.WaitOne(60_000));
        }

        [Test]
        public async Task When_Rabbit_Is_Not_Responding_We_Throw()
        {
            var moqLogger = new Mock<ILogger<RabbitMqMetricsConsumer>>();

            var serviceProvider = InitServiceProvider(1, 1);

            var publisher = new TestPublisher();
            var processor = new DefaultMetricProcessor(new RabbitMqMetricsConsumer(RabbitMqMetricsConsumer.QueuePath,
                                                                                      new RabbitMetricsConfiguration(),
                                                                                      moqLogger.Object,
                                                                                      new QueueValueConverter(),
                                                                                      serviceProvider.GetService<IHttpClientFactory>()),
                                                        publisher);

            await RabbitMqTestFixture.RabbitTestContainer.StopAsync();

            Assert.ThrowsAsync<HttpRequestException>(async () => await processor.ProcessAsync());

            // start the container
            await RabbitMqTestFixture.RabbitTestContainer.StartAsync();
        }

        private static IServiceProvider InitServiceProvider(int maxRetries, int minDelayInSeconds)
        {
            var serviceCollection = new ServiceCollection();
            var config = new RabbitMetricsConfiguration();
            serviceCollection.AddMetricsConsumer(config.RabbitMqUserName, config.RabbitMqPassword, maxRetries, minDelayInSeconds);
            serviceCollection.AddLogging();
            return serviceCollection.BuildServiceProvider();
        }
    }
}
