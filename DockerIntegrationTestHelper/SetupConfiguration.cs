using System;
using System.Collections.Generic;
using System.Text;

namespace DockerIntegrationTestHelper
{
    public class SetupConfiguration
    {
        public string ContainerName { get; set; }
        public IList<PortConfiguration> PortMappings { get; set; }
        public string ImageName { get; set; }
        public string ImageTag { get; set; }
    }
}
