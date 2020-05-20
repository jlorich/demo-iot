using Microsoft.Extensions.Logging;

namespace MicrosoftSolutions.IoT.Demos.Device.Options {
    /// <summary>
    /// Device authentication methods
    /// </summary>
    public enum AuthenticationMethod {
        Unknown,
        X509,
        SymmetricKey

    };

    /// <summary>
    /// Device registraion methods
    /// </summary>
    public enum RegistrationMethod {
        Unknown,
        Simple,
        DeviceProvisioningService
    }

    /// <summary>
    /// This class represents overall configuration options for this application
    /// </summary>
    public class ConfigurationOptions {
        public AuthenticationMethod AuthenticationMethod { get; set; }

        public RegistrationMethod RegistrationMethod { get; set; }

        public string RpcMethodName { get; set; }

        public LogLevel LogLevel { get; set; }
    }
}