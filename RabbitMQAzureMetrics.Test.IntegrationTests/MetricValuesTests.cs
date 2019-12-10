using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
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
    public class MetricValuesTests
    {

        [Test]
        public async Task Run_Statistics_Should_Expose_All_Collected_QueueMetrics()
        {
            var publisher = new TestPublisher();
            var processor = new DefaultMetricProcessor(new RabbitMqMetricsConsumer(RabbitMqMetricsConsumer.QueuePath,
                                                                                      new RabbitMetricsConfiguration(),
                                                                                      serviceProvider.GetService<ILogger<RabbitMqMetricsConsumer>>(),
                                                                                      new QueueValueConverter(),
                                                                                      serviceProvider.GetService<IHttpClientFactory>()),
                                                        publisher);
            await ValidateMetrics(publisher, processor, QueueValueConverter.PublishedMetrics);
        }

        [Test]
        public async Task Run_Statistics_Should_Expose_All_Collected_ExchangeMetrics()
        {
            var publisher = new TestPublisher();
            var processor = new DefaultMetricProcessor(new RabbitMqMetricsConsumer(RabbitMqMetricsConsumer.ExchangePath,
                                                                                      new RabbitMetricsConfiguration(),
                                                                                      serviceProvider.GetService<ILogger<RabbitMqMetricsConsumer>>(),
                                                                                      new ExchangeValueConverter(),
                                                                                      serviceProvider.GetService<IHttpClientFactory>()),
                                                        publisher);
            await ValidateMetrics(publisher, processor, ExchangeValueConverter.PublishedMetrics);
        }

        [Test]
        public async Task Run_Statistics_Should_Expose_All_Collected_OverviewMetrics()
        {
            var publisher = new TestPublisher();
            var processor = new DefaultMetricProcessor(new RabbitMqMetricsConsumer(RabbitMqMetricsConsumer.OverviewPath,
                                                                                      new RabbitMetricsConfiguration(),
                                                                                      serviceProvider.GetService<ILogger<RabbitMqMetricsConsumer>>(),
                                                                                      new MessageOverviewValueConverter(),
                                                                                      serviceProvider.GetService<IHttpClientFactory>()),
                                                        publisher);
            await ValidateMetrics(publisher, processor, MessageOverviewValueConverter.PublishedMetrics);
        }

        [Test]
        [Ignore("use for local debugging only to have metrics being generated")]
        public async Task Metrics_Generator()
        {
            await Task.Delay(60_000);
        }

        private async Task ValidateMetrics(TestPublisher publisher, DefaultMetricProcessor processor, IList<string> expectedMetrics)
        {
            var reportedMetrics = new HashSet<string>();
            const int metricNameIndex = 0;
            var mre = new ManualResetEvent(false);

            publisher.MetricsPublished += (c) =>
            {
                foreach (var v in c.Values)
                {
                    if (expectedMetrics.Contains(v.Dimensions[metricNameIndex]) && !reportedMetrics.Contains(v.Dimensions[metricNameIndex]))
                    {
                        reportedMetrics.Add(v.Dimensions[metricNameIndex]);
                    }
                }

                if (reportedMetrics.Count == expectedMetrics.Count)
                {
                    mre.Set();
                }
            };

            var run = true;
            var runner = Task.Run(async () =>
            {
                while (run)
                {
                    try
                    {
                        await Task.Delay(1000);
                        await processor.ProcessAsync();

                        rabbitMqStatsGenerator.PauseConsumer();
                        await Task.Delay(1000);

                        await processor.ProcessAsync();
                    }
                    finally
                    {
                        rabbitMqStatsGenerator.ResumeConsumer();
                    }
                }
            });

            var result = mre.WaitOne(60_000);

            // tear down
            run = false;
            await runner;

            if (!result)
            {
                Assert.Fail($"Not all metrics were reported. Missing: {string.Join(", ", expectedMetrics.Where(x => !reportedMetrics.Contains(x)))}");
            }
        }

        private RabbitMqStatsGenerator rabbitMqStatsGenerator;
        private IServiceProvider serviceProvider;

        [SetUp]
        public async Task Setup()
        {
            rabbitMqStatsGenerator = new RabbitMqStatsGenerator();
            await rabbitMqStatsGenerator.StartAsync();

            var serviceCollection = new ServiceCollection();
            var config = new RabbitMetricsConfiguration();
            serviceCollection.AddMetricsConsumer(config.RabbitMqUserName, config.RabbitMqPassword);
            serviceCollection.AddLogging();
            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [TearDown]
        public async Task TearDown()
        {
            await rabbitMqStatsGenerator.StopAsync();
        }
    }
}