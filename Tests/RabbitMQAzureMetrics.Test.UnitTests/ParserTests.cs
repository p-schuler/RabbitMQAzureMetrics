using NUnit.Framework;
using RabbitMQAzureMetrics.MetricsValueConverters;
using RabbitMQAzureMetrics.ValuePublishers.Overview;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Test.UnitTests
{
    public class ParserTests
    {
        const string RabbitMqVersion = "3.8.2";
        const float DefaultValue = 99.1f;
        static readonly string[] valuesToSkip = new string[] { "Rate: delivery delay" };

        [Test]
        [TestCaseSource(typeof(ParserTests), nameof(ParserSource))]
        public async Task When_Parsing_All_Values_Should_Be_Read(string resourceFile, IMetricsValueConverter converter)
        {
            var json = await ReadJsonContentAsync("queues.json");
            var queueValueConverter = new QueueValueConverter();
            var result = queueValueConverter.Convert(json);

            foreach (var val in result.Values)
            {
                var label = val.Dimensions[0];
                if (Array.IndexOf(valuesToSkip, label) != -1)
                    continue;

                Assert.That(NearlyEqual(DefaultValue, val.Value), $"Invalid value for '{val.Dimensions[0]}'");
            }
        }

        static IEnumerable<object[]> ParserSource
        {
            get
            {
                yield return new object[] { "queues.json", new QueueValueConverter() };
                yield return new object[] { "exchanges.json", new ExchangeValueConverter() };
                yield return new object[] { "overview.json", new MessageOverviewValueConverter() };
            }
        }

        private Task<string> ReadJsonContentAsync(string resourceFile)
        {
            var filePath = Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).ToString(), "Inputfiles", RabbitMqVersion,  resourceFile);
            return File.ReadAllTextAsync(filePath);
        }

        private static bool NearlyEqual(double f1, double f2)
        {
            const double variance = 0.0000001d;
            return Math.Abs(f1 - f2) < variance;
        }

        [SetUp]
        public void Setup()
        {
        }
    }
}
