using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

namespace MicrosoftSolutions.IoT.Demos.Device.DeviceClientFactories {
    public interface IDeviceClientFactory {
        Task<DeviceClient> Create();
    }
}