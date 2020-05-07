using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using MicrosoftSolutions.IoT.Demos.Options;

namespace MicrosoftSolutions.IoT.Demos.DeviceRegistrationProviders {

    // Provides device registration information from the IoT Hub Device Provisioning Service
    public class DeviceRegistrationDpsProvider : IDeviceRegistrationProvider {
        private DeviceRegistrationDpsOptions _options;
        private SecurityProvider _securityProvider;
        private DeviceRegistrationResult _registrationResult;

        public DeviceRegistrationDpsProvider(
            IOptions<DeviceRegistrationDpsOptions> options,
            SecurityProvider securityProvider
        ) {
            _options = options.Value;
            _securityProvider = securityProvider;
        }

        public string AssignedHub {
            get {
                return _registrationResult.AssignedHub;
            }
        }

        public string DeviceId {
            get {
                return _registrationResult.DeviceId;
            }
        }

        // Registers a device with the Device Provisioning Service using X509 Certificate Authentication
        public async Task Register() {
            ProvisioningTransportHandler transport;
            
            switch(_options.TransportMethod) {
                case TransportCommunicationMethods.Http:
                    transport = new ProvisioningTransportHandlerHttp();
                    break;
                case TransportCommunicationMethods.Mqtt:
                    transport = new ProvisioningTransportHandlerMqtt();
                    break;
                case TransportCommunicationMethods.Amqp:
                    transport = new ProvisioningTransportHandlerAmqp();
                    break;
                default:
                    throw new Exception("Unknown DPS Transport Method");
            } 

            // Create the provisioning client
            var dpsClient = ProvisioningDeviceClient.Create(
                _options.GlobalDeviceEndpoint,
                _options.IdScope,
                _securityProvider,
                transport
            );

            // Register the device
            _registrationResult = await dpsClient.RegisterAsync();
        }
    }
}