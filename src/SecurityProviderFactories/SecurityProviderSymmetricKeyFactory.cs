using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using MicrosoftSolutions.IoT.Demos.Options;

namespace MicrosoftSolutions.IoT.Demos.SecurityProviderFactories {

    public class SecurityProviderSymmetricKeyFactory : ISecurityProviderFactory {
        SecurityProviderSymmetricKeyOptions _options;

        public SecurityProviderSymmetricKeyFactory(IOptions<SecurityProviderSymmetricKeyOptions> options) {
            _options = options.Value;
        }

        public SecurityProvider Create() {
            var securityProvider = new SecurityProviderSymmetricKey(
                _options.RegistrationId,
                _options.PrimaryKey,
                _options.SecondaryKey
            );

            return securityProvider;
        }
    }
}