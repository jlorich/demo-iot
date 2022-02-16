using Microsoft.Azure.Devices.Client;

namespace MicrosoftSolutions.IoT.Demos.Device.Options {
    public class TransportOptions {
        public string ProxyUri { get; set; }

        public TransportType TransportType { get; set; }
    }
}