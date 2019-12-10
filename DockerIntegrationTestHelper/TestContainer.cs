using Docker.DotNet;
using Docker.DotNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DockerIntegrationTestHelper
{
    public class TestContainer : IDisposable
    {
        const string DockerConnectionWin = "npipe://./pipe/docker_engine";
        const string DockerConnectionLinux = "unix:///var/run/docker.sock";
        const string DockerRunningState = "running";

        public string ContainerId { get; set; }

        private readonly string dockerConnection;
        private readonly SetupConfiguration configuration;

        public TestContainer(string dockerConnection, SetupConfiguration configuration)
        {
            this.dockerConnection = dockerConnection;
            this.configuration = configuration;
        }

        public async static Task<TestContainer> CreateAsync(SetupConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var dockerConnection = Environment.OSVersion.Platform.ToString().Contains("Win", StringComparison.OrdinalIgnoreCase) ?
                    DockerConnectionWin :
                    DockerConnectionLinux;

            var container = new TestContainer(dockerConnection, config);
            await container.CreateAsync();
            return container;
        }

        private async Task CreateAsync()
        {
            Console.WriteLine("Starting container");

            using (var conf = new DockerClientConfiguration(new Uri(dockerConnection))) // localhost
            using (var client = conf.CreateClient())
            {
                Console.WriteLine("Starting container...");
                var containers = await client.Containers.ListContainersAsync(new ContainersListParameters() { All = true });
                Console.WriteLine("listing container...");
                var container = containers.FirstOrDefault(c => c.Names.Contains("/" + this.configuration.ContainerName));

                var isRunning = false;

                if (container == null)
                {
                    await CreateContainerAsync(client);
                }
                else
                {
                    this.ContainerId = container.ID;

                    isRunning = (container.State == DockerRunningState);
                }

                if (!isRunning)
                {
                    Console.WriteLine("Starting container...");

                    var started = await client.Containers.StartContainerAsync(this.ContainerId, new ContainerStartParameters());
                    if (!started)
                    {
                        throw new InvalidOperationException("Failed to start container");
                    }
                }

                Console.WriteLine("Container is running with Id {0}", this.ContainerId);
            }
        }

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(this.ContainerId)) throw new InvalidOperationException("Container not created");
            Console.WriteLine("Starting container...");

            using (var conf = new DockerClientConfiguration(new Uri(dockerConnection))) // localhost
            using (var client = conf.CreateClient())
            {
                var started = await client.Containers.StartContainerAsync(this.ContainerId, new ContainerStartParameters());
                if (!started)
                {
                    throw new InvalidOperationException("Failed to start container");
                }
            }
        }

        public async Task StopAsync()
        {
            if (string.IsNullOrEmpty(this.ContainerId)) throw new InvalidOperationException("Container not created");
            Console.WriteLine("Starting container...");

            using (var conf = new DockerClientConfiguration(new Uri(dockerConnection))) // localhost
            using (var client = conf.CreateClient())
            {
                var started = await client.Containers.StopContainerAsync(this.ContainerId, new ContainerStopParameters());
                if (!started)
                {
                    throw new InvalidOperationException("Failed to stop container");
                }
            }
        }

        private async Task CreateContainerAsync(DockerClient client)
        {
            // Download image
            await client.Images.CreateImageAsync(new ImagesCreateParameters()
            {
                FromImage = this.configuration.ImageName,
                Tag = this.configuration.ImageTag
            }, new AuthConfig(), new Progress<JSONMessage>());

            var config = new Config()
            {
                Hostname = "localhost"
            };

            // Configure the ports to expose
            var hostConfig = new HostConfig();
            if (this.configuration.PortMappings?.Count > 0)
            {
                hostConfig.PortBindings = new Dictionary<string, IList<PortBinding>>(this.configuration.PortMappings.Count);
                foreach (var portConfig in this.configuration.PortMappings)
                {
                    hostConfig.PortBindings.Add($"{portConfig.Port}/{portConfig.Protocol}", new List<PortBinding> { new PortBinding { HostIP = "127.0.0.1", HostPort = portConfig.HostPort.HasValue ? portConfig.HostPort.ToString() : portConfig.Port.ToString() } });
                }
            }

            Console.WriteLine("Creating container...");
            // Create the container
            var response = await client.Containers.CreateContainerAsync(new CreateContainerParameters(config)
            {
                Image = this.configuration.ImageName + ":" + this.configuration.ImageTag,
                Name = this.configuration.ContainerName,
                Tty = false,
                HostConfig = hostConfig
            });
            this.ContainerId = response.ID;
        }

        public async Task DestroyAsync()
        {
            if (this.ContainerId == null) return;
            using (var conf = new DockerClientConfiguration(new Uri(this.dockerConnection)))
            using (var client = conf.CreateClient())
            {
                await client.Containers.RemoveContainerAsync(this.ContainerId, new ContainerRemoveParameters()
                {
                    Force = true
                });

                this.ContainerId = null;
            }
        }

        public void Dispose()
        {
            DestroyAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }
}
