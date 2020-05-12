using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Common.Models;
using MicrosoftSolutions.IoT.Demos.Device.DeviceRegistrationProviders;
using MicrosoftSolutions.IoT.Demos.Rpc;

namespace MicrosoftSolutions.IoT.Demos.Device {
    public class SampleDevice {

        private IDeviceRegistrationProvider _deviceRegistrationProvider;
        private DeviceClient _deviceClient;
        private RpcClient _rpcClient;

        public SampleDevice(
            IDeviceRegistrationProvider deviceRegistrationProvider,
            DeviceClient deviceClient,
            RpcClient rpcClient
        ) {
            _deviceRegistrationProvider = deviceRegistrationProvider;
            _deviceClient = deviceClient;
            _rpcClient = rpcClient;

            Task.Run(async () => {
                await ReceiveCommandsAsync();
            });
        }

        private async Task ReceiveCommandsAsync()
        {
            Message receivedMessage;
            string messageData;

            while (true)
            {
                try{
                    receivedMessage = await _deviceClient.ReceiveAsync().ConfigureAwait(false);

                    if (receivedMessage != null)
                    {
                        messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());

                        _rpcClient.Dispatch(messageData);

                        await _deviceClient.CompleteAsync(receivedMessage).ConfigureAwait(false);
                    }
                } catch (Exception e) {
                    Console.WriteLine(e.ToString());
                }
                
                await Task.Delay(1000);
            }
        }

        // Sends a simple telemetry message to an IoT DeviceClient
        public async Task SendTelemetryMessage() {
            var message = new UserActionCreateRequest() {
                User = new User() {
                    FirstName = "Carole",
                    LastName = "Baskin"
                },
                Action = UserActions.KILLED_HER_HUSBAND
            };

            await _rpcClient.SendAsync(message);

        }

        // Sends a request/response message using a message dispatcher
        public async Task<UserGetResponse> GetUser(UserGetRequest request) {
            return await _rpcClient.CallAsync<UserGetResponse>(request);
        }

        public async Task UploadTestFileToStorage() {
            using (var sourceData = new FileStream(@"fileUploadTest.txt", FileMode.Open))
            {
                var fileName = $"fileUploadTest-{DateTime.UtcNow}.txt";
                await _deviceClient.UploadToBlobAsync(fileName, sourceData);
            }
    
        }
    }
}