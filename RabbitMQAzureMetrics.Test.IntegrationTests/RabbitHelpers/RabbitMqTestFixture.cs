using DockerIntegrationTestHelper;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitMQAzureMetrics.Test.IntegrationTests
{
    [SetUpFixture]
    public class RabbitMqTestFixture
    {
        public static TestContainer RabbitTestContainer { get; private set; }

        [OneTimeSetUp]
        public static async Task SetupRabbitMqAsync()
        {
            if (RabbitTestContainer != null) 
                throw new InvalidOperationException("already initialized");

            var config = new SetupConfiguration 
            {
                 ContainerName = "rabbitmqpublisherintegrationtests",
                 ImageName = "rabbitmq",
                 ImageTag = "3-management",
                 PortMappings = new List<PortConfiguration> { new PortConfiguration(15672), new PortConfiguration(5672)}
            };

            RabbitTestContainer = await TestContainer.CreateAsync(config);
        }

        [OneTimeTearDown]
        public static async Task TeardownRabbitMqAsync()
        {
            await RabbitTestContainer.DestroyAsync();
            RabbitTestContainer.Dispose();
            RabbitTestContainer = null;
        }
    }
}
