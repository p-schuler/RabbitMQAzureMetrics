using System;
using System.Collections.Generic;
using System.Text;

namespace DockerIntegrationTestHelper
{
    public class PortConfiguration
    {
        public PortConfiguration() { }
        public PortConfiguration(int port) 
        {
            this.Port = port;
        }

        public int Port { get; set; }
        public int? HostPort { get; set; }
        public string Protocol { get; set; } = "tcp";
    }
}
