using System;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using MicrosoftSolutions.IoT.Demos.Rpc;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Common.Models;
using MicrosoftSolutions.IoT.Demos.Service.Services;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace MicrosoftSolutions.IoT.Demos.Service.Functions {
    public class EventGridRpcFunction {
        private ServiceClient _ServiceClient;
        private ISampleService _SampleService;

        public EventGridRpcFunction(
            ServiceClient serviceClient,
            ISampleService sampleService
        ) {
            _ServiceClient = serviceClient;
           _SampleService = sampleService;
        }

        private struct IoTHubMessage {
            public string DeviceId;
            public string Message;
        }


        [FunctionName("Rpc")]
        public async Task Rpc([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log) {
            var messageData = GetMessageDataFromEvent(eventGridEvent);

            var handler = new DispatchingClientMessageHandler();

            // Configure message sending
            handler.SendAsync = async (clientId, message) => {
                var method = new CloudToDeviceMethod("rpc");
                method.SetPayloadJson(message);

                await _ServiceClient.InvokeDeviceMethodAsync(clientId, method);
            };

            var jsonRpc = new JsonRpc(handler);
                
            jsonRpc.AddLocalRpcTarget(_SampleService);

            handler.Dispatch(messageData.DeviceId, messageData.Message);

            await jsonRpc.Completion;
        }
        
        private IoTHubMessage GetMessageDataFromEvent(EventGridEvent eventGridEvent) {
            var base64String = ((JObject)eventGridEvent.Data)["body"].ToString();
            var byteArray = Convert.FromBase64String(base64String);
            var message = Encoding.UTF8.GetString(byteArray);
            var deviceId = "";

            return new IoTHubMessage() {
                DeviceId = deviceId,
                Message = message
            };
        }
    }
}
