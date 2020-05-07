using System.Configuration;

namespace MicrosoftSolutions.IoT.Demos.Options {
    public class DeviceRegistrationDpsOptions {
        public string GlobalDeviceEndpoint { get; set; }

        public string IdScope { get; set; }

        public string RegistrationId { get; set; }
        
        public TransportCommunicationMethods TransportMethod { get; set; }
    }
}