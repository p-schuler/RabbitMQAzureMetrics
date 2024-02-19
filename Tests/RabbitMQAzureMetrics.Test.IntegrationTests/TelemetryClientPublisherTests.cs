using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Metrics;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using RabbitMQAzureMetrics.Consumer;
using RabbitMQAzureMetrics.MetricsValueConverters;
using RabbitMQAzureMetrics.Processors;
using RabbitMQAzureMetrics.ValuePublishers.AppInsight;
using RabbitMQAzureMetrics.ValuePublishers.Overview;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;

namespace RabbitMQAzureMetrics.Test.IntegrationTests
{
    public class TelemetryClientPublisherTests
    {
        [Test]
        [TestCaseSource(typeof(TelemetryClientPublisherTests), nameof(DimensionsSource))]
        public void When_Calling_TelemetryClient_With_Multiple_Dimensions_Metrics_Are_Accepted(string[] dimensions)
        {
            var metricIdentifier = CreateMetricIdentifier(dimensions);
            var tc = new TelemetryClient(TelemetryConfiguration.CreateDefault());
            var metric = tc.GetMetric(metricIdentifier);
            var publisher = new TelemetryClientPublisher(metric, Mock.Of<ILogger>());
            var collectionWrapper = new MetricValueCollectionWrapper();
            collectionWrapper.Add(0, dimensions);

            Assert.DoesNotThrowAsync(async () => await publisher.PublishAsync(collectionWrapper));
        }

        private static MetricIdentifier CreateMetricIdentifier(string[] dimensions)
        {
            const string MetricNamespace = "ns";
            const string MetricName = "test";

            var typeofMetricIdentifier = typeof(MetricIdentifier);
            var constructorTypes = new Type[2 + dimensions.Length];

            for (var i = 0; i < constructorTypes.Length; i++)
            {
                constructorTypes[i] = typeof(string);
            }

            var ctor = typeofMetricIdentifier.GetConstructor(constructorTypes);
            var ctorValues = new object[constructorTypes.Length];
            ctorValues[0] = MetricNamespace;
            ctorValues[1] = MetricName;
            for (var i = 2; i < ctorValues.Length; i++)
            {
                ctorValues[i] = dimensions[i - 2];
            }

            return (MetricIdentifier) ctor.Invoke(ctorValues);
        }

        static IEnumerable<object[]> DimensionsSource
        {
            get
            {
                const int MaxNumberOfDimensions = 10;
                List<string> dimensions = new List<string>(MaxNumberOfDimensions);
                for (var i = 0; i < MaxNumberOfDimensions; i++) 
                {
                    dimensions.Add(i.ToString());
                    yield return new object[] { dimensions.ToArray() };
                }
            }
        }
    }
}
