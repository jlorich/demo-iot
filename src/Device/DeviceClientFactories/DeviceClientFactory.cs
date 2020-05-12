using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using MicrosoftSolutions.IoT.Demos.Device.DeviceRegistrationProviders;
using MicrosoftSolutions.IoT.Demos.Device.Options;

namespace MicrosoftSolutions.IoT.Demos.Device.DeviceClientFactories {

    // A demo that highlights connectiong to DPS with an X509 certificate
    public class DeviceClientFactory : IDeviceClientFactory {
        SecurityProvider _securityProvider;

        IAuthenticationMethod _authenticationMethod;

        IDeviceRegistrationProvider _registrationProvider;

        private ProxyOptions _proxyOptions;

        public DeviceClientFactory(
            SecurityProvider securityProvider,
            IAuthenticationMethod authenticationMethod,
            IDeviceRegistrationProvider registrationProvider,
            IOptions<ProxyOptions> proxyOptions
        ) {
            _securityProvider = securityProvider;
            _authenticationMethod = authenticationMethod;
            _registrationProvider = registrationProvider;
            _proxyOptions = proxyOptions.Value;
        }

        public async Task<DeviceClient> Create() {
            await _registrationProvider.Register();

            // Set up a proxy for DPS if specified
            if (!String.IsNullOrEmpty(_proxyOptions?.ProxyUri)) {
                // Configure IoT Hub/Central Proxy
                Http1TransportSettings transportSettings = new Http1TransportSettings();
                transportSettings.Proxy = new WebProxy(_proxyOptions.ProxyUri);
                ITransportSettings[] transportSettingsArray = new ITransportSettings[] { transportSettings };

                return DeviceClient.Create(
                    _registrationProvider.AssignedHub,
                    _authenticationMethod,
                    transportSettingsArray
                );
            }

            return DeviceClient.Create(
                _registrationProvider.AssignedHub,
                _authenticationMethod
            );
        }
    }
}