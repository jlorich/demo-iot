using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Common.Enumerations;
using MicrosoftSolutions.IoT.Demos.DeviceRegistrationProviders;

namespace MicrosoftSolutions.IoT.Demos.Common {
    public class SampleDevice {

        private IDeviceRegistrationProvider _deviceRegistrationProvider;
        private DeviceClient _deviceClient;
        

        public SampleDevice(IDeviceRegistrationProvider deviceRegistrationProvider, DeviceClient deviceClient) {
            _deviceRegistrationProvider = deviceRegistrationProvider;
            _deviceClient = deviceClient;
        }

        // Sends a simple telemetry message to an IoT DeviceClient
        public async Task SendTelemetryMessage() {
            var message = new SampleTelemetryMessage() {
                User = new SampleUser() {
                    FirstName = "Carole",
                    LastName = "Baskin"
                },
                Action = SampleUserActions.KILLED_HER_HUSBAND
            };

            var messageText = JsonSerializer.Serialize(message);
            
            var iotMessage = new Message(Encoding.UTF8.GetBytes(messageText));

            await _deviceClient.SendEventAsync(iotMessage);
            
        }
    }
}