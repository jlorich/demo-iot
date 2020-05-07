using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using MicrosoftSolutions.IoT.Demos.DeviceRegistrationProviders;

namespace MicrosoftSolutions.IoT.Demos.DeviceClientFactories {

    // A demo that highlights connectiong to DPS with an X509 certificate
    public class DeviceClientFactory : IDeviceClientFactory {
        SecurityProvider _securityProvider;
        IAuthenticationMethod _authenticationMethod;
        IDeviceRegistrationProvider _registrationProvider;

        public DeviceClientFactory(
            SecurityProvider securityProvider,
            IAuthenticationMethod authenticationMethod,
            IDeviceRegistrationProvider registrationProvider
        ) {
            _securityProvider = securityProvider;
            _authenticationMethod = authenticationMethod;
            _registrationProvider = registrationProvider;
        }

        public async Task<DeviceClient> Create() {
            await _registrationProvider.Register();

            var deviceClient = DeviceClient.Create(
                _registrationProvider.AssignedHub,
                _authenticationMethod
            );

            return deviceClient;
        }
    }
}