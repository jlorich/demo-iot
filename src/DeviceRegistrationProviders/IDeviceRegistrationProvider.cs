using System.Threading.Tasks;
using Microsoft.Azure.Devices.Provisioning.Client;

namespace MicrosoftSolutions.IoT.Demos.DeviceRegistrationProviders {

    // A demo that highlights connectiong to DPS with an X509 certificate
    public interface IDeviceRegistrationProvider {
        Task Register();

        string AssignedHub {
            get;
        }

        string DeviceId {
            get;
        }
    }
}