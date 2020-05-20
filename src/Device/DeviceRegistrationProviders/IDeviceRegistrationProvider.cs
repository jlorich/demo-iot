using System.Threading.Tasks;
using Microsoft.Azure.Devices.Provisioning.Client;

namespace MicrosoftSolutions.IoT.Demos.Device.DeviceRegistrationProviders {

    // A demo that highlights connectiong to DPS with an X509 certificate
    public interface IDeviceRegistrationProvider {
        Task RegisterAsync();

        string AssignedHub {
            get;
        }

        string DeviceId {
            get;
        }
    }
}