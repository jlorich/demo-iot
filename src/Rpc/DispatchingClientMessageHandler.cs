using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks; 
using Microsoft.VisualStudio.Threading;
using Newtonsoft.Json;
using StreamJsonRpc;
using StreamJsonRpc.Protocol;

namespace MicrosoftSolutions.IoT.Demos.Rpc {
    public class DispatchingClientMessageHandler : IJsonRpcMessageHandler {
        private struct ClientMessage
        {
            public ClientMessage(string clientId, string message) {
                ClientId = clientId;
                Message = message;
            }

            public string Message { get; }
            public string ClientId { get; }
        }

        public bool CanRead { get; } = true;

        public bool CanWrite { get; } = true;

        private AsyncQueue<ClientMessage> _Queue = new AsyncQueue<ClientMessage>();


        public IJsonRpcMessageFormatter Formatter { get; } = new JsonMessageFormatter();

        public Func<string, string, Task> SendAsync { get; set; }

        public void Dispatch(string clientId, string message) {
            var clientMessage = new ClientMessage(clientId, message);
            _Queue.Enqueue(clientMessage);
        }

        public async ValueTask<JsonRpcMessage> ReadAsync(CancellationToken cancellationToken) {
            while(cancellationToken.IsCancellationRequested) {
                var clientMessage = await _Queue.DequeueAsync();

                Console.WriteLine($"Received message from client: {clientMessage.ClientId}");
                Console.WriteLine(clientMessage.Message);

                var bytes = Encoding.UTF8.GetBytes(clientMessage.Message);
                var sequence = new ReadOnlySequence<byte>(bytes);
                var jsonRpcMessage = Formatter.Deserialize(sequence);

                // Append Client Id on to the message if it's a request so we can use the appropriate channel
                if (jsonRpcMessage is JsonRpcRequest request && request.IsResponseExpected) {
                    request.RequestId = new RequestId($"{clientMessage.ClientId}::{request.RequestId.ToString()}");
                }

                return jsonRpcMessage;
            }

            return null;
        }

        public async ValueTask WriteAsync(JsonRpcMessage jsonRpcMessage, CancellationToken cancellationToken) {
            string clientId = null;

            // Extract and strip Client Id from the result if appropriate
            if (jsonRpcMessage is JsonRpcResult result && !result.RequestId.IsEmpty) {
                var splitContent = result.RequestId.ToString().Split("::", 2);
                clientId = splitContent[0];
                result.RequestId = ParseRequestId(splitContent[1]);
            }

            var pipe = new Pipe();
            this.Formatter.Serialize(pipe.Writer, jsonRpcMessage);
            await pipe.Writer.CompleteAsync();

            var stream = pipe.Reader.AsStream();
            var reader = new StreamReader(stream);
            var stringResult = await reader.ReadToEndAsync();

            Console.WriteLine($"Sending message from client it: {clientId}");
            Console.WriteLine(stringResult);
            
            await SendAsync(clientId, stringResult);
        }

        private static RequestId ParseRequestId(object value)
        {
            return
                value is null ? default :
                value is long l ? new RequestId(l) :
                value is string s ? new RequestId(s) :
                value is int i ? new RequestId(i) :
                throw new JsonSerializationException("Unexpected type for id property: " + value.GetType().Name);
        }
    }
}