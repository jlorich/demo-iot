using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace MicrosoftSolutions.IoT.Demos.DeviceAuthenticationFactories {

    // A demo that highlights connectiong to DPS with an X509 certificate
    public interface IDeviceAuthenticationFactory {
        Task<IAuthenticationMethod> Create();
    }
}