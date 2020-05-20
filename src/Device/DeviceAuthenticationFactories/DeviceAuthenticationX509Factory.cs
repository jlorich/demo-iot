using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using MicrosoftSolutions.IoT.Demos.Device.DeviceAuthenticationFactories;
using MicrosoftSolutions.IoT.Demos.Device.DeviceRegistrationProviders;

namespace MicrosoftSolutions.IoT.Demos.Device.SecurityProviderFactories {

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

        public IAuthenticationMethod Create() {
            return new DeviceAuthenticationWithX509Certificate(
                _deviceRegistrationProvider.DeviceId,
                _securityProvider.GetAuthenticationCertificate());
        }
    }
}

