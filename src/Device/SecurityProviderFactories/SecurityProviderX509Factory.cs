using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using MicrosoftSolutions.IoT.Demos.Device.Options;

namespace MicrosoftSolutions.IoT.Demos.Device.SecurityProviderFactories {

    public class SecurityProviderX509Factory : ISecurityProviderFactory {
        SecurityProviderX509Options _options;

        public SecurityProviderX509Factory(IOptions<SecurityProviderX509Options> options) {
            _options = options.Value;
        }

        public SecurityProvider Create() {
            var certificate = LoadX509Certificate(_options.CertificateFileName);
            return new SecurityProviderX509Certificate(certificate);
        }

        private X509Certificate2 LoadX509Certificate(string fileName) {
            var certificateCollection = new X509Certificate2Collection();

            certificateCollection.Import(fileName);

            // Loop through the certificate file until we have a certificate with an available private key
            foreach (X509Certificate2 element in certificateCollection) {
                if (element.HasPrivateKey) {
                    return element;
                }
            }

            throw new FileNotFoundException($"cert did not contain any certificate with a private key.");
        }
    }
}