// using System;
// using System.Buffers;
// using System.IO;
// using System.IO.Pipelines;
// using System.Threading;
// using System.Threading.Tasks; 
// using Microsoft.Azure.Devices.Client;
// using StreamJsonRpc;
// using StreamJsonRpc.Protocol;

// namespace MicrosoftSolutions.IoT.Demos.Rpc {
//     public class IoTJsonRpcMessageHandler : IJsonRpcMessageHandler {

//         public bool CanRead { get; } = true;

//         public bool CanWrite { get; } = true;

//         private DeviceClient _Client;


//         public IoTJsonRpcMessageHandler(DeviceClient client) {
//             _Client = client;
//         }

//         public IJsonRpcMessageFormatter Formatter { get; } = new JsonMessageFormatter();

//         public async ValueTask<JsonRpcMessage> ReadAsync(CancellationToken cancellationToken) {
//             while(cancellationToken.IsCancellationRequested) {
//                 // This will by default wait up to 4 minutes to receive a message
//                 var receivedMessage = await _Client.ReceiveAsync().ConfigureAwait(false);

//                 if (receivedMessage != null)
//                 {
//                     var bytes = receivedMessage.GetBytes();
//                     var sequence = new ReadOnlySequence<byte>(bytes);
//                     var jsonRpcMessage = Formatter.Deserialize(sequence);

//                     await _Client.CompleteAsync(receivedMessage).ConfigureAwait(false);

//                     return jsonRpcMessage;
//                 }

//                 await Task.Delay(1000);
//             }

//             return null;
//         }

//         public async ValueTask WriteAsync(JsonRpcMessage content, CancellationToken cancellationToken) {
//             var pipe = new Pipe();
//             this.Formatter.Serialize(pipe.Writer, content);
//             await pipe.Writer.CompleteAsync();

//             var stream = pipe.Reader.AsStream();
//             var reader = new StreamReader(stream);
//             var res = await reader.ReadToEndAsync();

            
//             Console.WriteLine(res);
//         }
//     }
// }