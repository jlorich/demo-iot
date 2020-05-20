using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Rpc;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace MicrosoftSolutions.IoT.Demos.Service.Functions {

    /// <summary>
    ///  This class handles RPC messages sent over EventGrid
    /// </summary>
    public class EventGridRpcFunction {
        JsonRpc _JsonRpc;
        DispatchingClientMessageHandler _MessageHandler;
        IRpcService _RpcService;

        /// <summary>
        /// Constructs a new EventGridRpcFunction class
        /// </summary>
        /// <param name="jsonRpc">The JsonRpc instance we'll use monitor for message processing completion</param>
        /// <param name="messageHandler">The message handler we'll use to dispatch Message to the RPC service</param>
        /// <param name="rpcService">
        /// The service instance the RPC will call against.  This is not needed to be used, but we need to have 
        /// a constructor reference to it so DI will generate it and the JsonRpc instance can call methods against it.
        ///</param>
        public EventGridRpcFunction(
            JsonRpc jsonRpc,
            DispatchingClientMessageHandler messageHandler,
            IRpcService rpcService
        ) {
           _JsonRpc = jsonRpc;
           _MessageHandler = messageHandler;
           _RpcService = rpcService;
        }

        /// <summary>
        /// Azure Functions entrypoint for handling Remote Procedure Calls sent from an
        /// Azure IoT Device through EventGrid.
        ///
        /// This method receives an EventGrid Event, pulls the relevant data out of it, 
        /// hands it to the Rpc message handler, then awaits for JsonRpc to complete processing
        /// of the request.
        /// </summary>
        /// <param name="eventGridEvent">The RPC event to handle</param>
        /// <param name="log">A logger to use</param>
        [FunctionName("rpc")]
        public async Task Rpc([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log) {
            var messageData = GetMessageDataFromEvent(eventGridEvent);

            log.LogTrace($"Received message from {messageData.DeviceId}");
            log.LogTrace(messageData.Message);

            _MessageHandler.Dispatch(messageData.Message, messageData.DeviceId);
            _MessageHandler.Complete();

            await _JsonRpc.Completion;

            log.LogTrace($"Finished handling message from {messageData.DeviceId}");
        }
        
        /// <summary>
        /// Gets a device ID and message string from an EventGridEvent
        /// </summary>
        private (string DeviceId, string Message) GetMessageDataFromEvent(EventGridEvent eventGridEvent) {
            var base64String = ((JObject)eventGridEvent.Data)["body"].ToString();
            var byteArray = Convert.FromBase64String(base64String);
            var message = Encoding.UTF8.GetString(byteArray);
            var deviceId = eventGridEvent.Subject.Substring(8); // strip off "/devices"

            return (deviceId, message);
        }
    }
}
