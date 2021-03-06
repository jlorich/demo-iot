using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MicrosoftSolutions.IoT.Demos.Device.Options;

namespace MicrosoftSolutions.IoT.Demos.Device.DeviceRegistrationProviders {

    // Provides device registration information directly from configuration (i.e. no DPS)
    public class DeviceRegistrationSimpleProvider : IDeviceRegistrationProvider {
        DeviceRegistrationSimpleOptions _options;

        public DeviceRegistrationSimpleProvider(IOptions<DeviceRegistrationSimpleOptions> options) {
            _options = options.Value;
        }

        // Noop since we are not registering this device with DPS
        public async Task RegisterAsync() {
            await Task.CompletedTask;
        }

        public string AssignedHub {
            get {
                return _options.IoTHubUri;
            }
        }

        public string DeviceId {
            get {
                return _options.DeviceId;
            }
        }
    }
}