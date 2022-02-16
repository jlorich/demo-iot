using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using MicrosoftSolutions.IoT.Demos.Device.Options;

namespace MicrosoftSolutions.IoT.Demos.Device.DeviceRegistrationProviders {

    // Provides device registration information from the IoT Hub Device Provisioning Service
    public class DeviceRegistrationDpsProvider : IDeviceRegistrationProvider {
        private DeviceRegistrationDpsOptions _deviceRegistrationOptions;
        private TransportOptions _transportOptions;
        private SecurityProvider _securityProvider;
        private DeviceRegistrationResult _registrationResult;

        public DeviceRegistrationDpsProvider(
            IOptions<DeviceRegistrationDpsOptions> deviceRegistrationOptions,
            IOptions<TransportOptions> transportOptions,
            SecurityProvider securityProvider
        ) {
            _deviceRegistrationOptions = deviceRegistrationOptions.Value;
            _transportOptions = transportOptions.Value;
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
        public async Task RegisterAsync() {
            ProvisioningTransportHandler transport;
            
            switch(_deviceRegistrationOptions.TransportMethod) {
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

            // Set up a proxy for DPS if specified
            if (!String.IsNullOrEmpty(_transportOptions?.ProxyUri)) {
                transport.Proxy = new WebProxy(_transportOptions.ProxyUri);
            }

            // Create the provisioning client
            var dpsClient = ProvisioningDeviceClient.Create(
                _deviceRegistrationOptions.GlobalDeviceEndpoint,
                _deviceRegistrationOptions.IdScope,
                _securityProvider,
                transport
            );

            // Register the device
            _registrationResult = await dpsClient.RegisterAsync();
        }
    }
}