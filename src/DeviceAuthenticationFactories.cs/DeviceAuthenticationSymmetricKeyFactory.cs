using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using MicrosoftSolutions.IoT.Demos.DeviceAuthenticationFactories;
using MicrosoftSolutions.IoT.Demos.DeviceRegistrationProviders;

namespace MicrosoftSolutions.IoT.Demos.SecurityProviderFactories {

    public class DeviceAuthenticationSymmetricKeyFactory : IDeviceAuthenticationFactory {
        SecurityProviderSymmetricKey _securityProvider;
        IDeviceRegistrationProvider _deviceRegistrationProvider;

        public DeviceAuthenticationSymmetricKeyFactory(
            SecurityProvider securityProvider,
            IDeviceRegistrationProvider deviceRegistrationProvider
        ) {
            _securityProvider = (SecurityProviderSymmetricKey)securityProvider;
            _deviceRegistrationProvider = deviceRegistrationProvider;
        }

        public async Task<IAuthenticationMethod> Create() {
            await Task.CompletedTask;
            
            return new DeviceAuthenticationWithRegistrySymmetricKey(
                _deviceRegistrationProvider.DeviceId,
                _securityProvider.GetPrimaryKey());
        }
    }
}

