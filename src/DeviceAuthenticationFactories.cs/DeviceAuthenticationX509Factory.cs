using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using MicrosoftSolutions.IoT.Demos.DeviceAuthenticationFactories;
using MicrosoftSolutions.IoT.Demos.DeviceRegistrationProviders;

namespace MicrosoftSolutions.IoT.Demos.SecurityProviderFactories {

    public class DeviceAuthenticationX509Factory : IDeviceAuthenticationFactory {
        SecurityProviderX509Certificate _securityProvider;
        IDeviceRegistrationProvider _deviceRegistrationProvider;

        public DeviceAuthenticationX509Factory(
            SecurityProvider securityProvider,
            IDeviceRegistrationProvider deviceRegistrationProvider
        ) {
            _securityProvider = (SecurityProviderX509Certificate)securityProvider;
            _deviceRegistrationProvider = deviceRegistrationProvider;
        }

        public async Task<IAuthenticationMethod> Create() {
            await _deviceRegistrationProvider.Register();

            return new DeviceAuthenticationWithX509Certificate(
                _deviceRegistrationProvider.DeviceId,
                _securityProvider.GetAuthenticationCertificate());
        }
    }
}

