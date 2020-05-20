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
    /// <summary>
    ///  This class is an Rpc Message Handler for the StreamJsonRpc Library that
    ///  allows for external control of message dispatching and sending.
    ///
    ///  This class also supports temporarily adding State identifier to the Request Id
    ///  so that the client can identify the proper channel to send message back to.
    /// </summary>
    public class DispatchingClientMessageHandler : IJsonRpcMessageHandler {
        public bool CanRead { get; } = true;

        public bool CanWrite { get; } = true;

        public IJsonRpcMessageFormatter Formatter { get; } = new JsonMessageFormatter();

        //protected override ValueTask FlushAsync(CancellationToken cancellationToken) => default;

        private AsyncQueue<(string message, string state)> _Queue = new AsyncQueue<(string, string)>();

        public Func<string, string, Task> SendAsync { get; set; }

        /// <summary>
        ///  Constructs a new Dispatching Client Message Handler
        // /// </summary>
        // public DispatchingClientMessageHandler() : base(new JsonMessageFormatter()) {

        // }

        
        // ~DispatchingClientMessageHandler(){
        //     Console.WriteLine("KILLING IT");
        // }

        // public new void Dispose()
        // {
        //     Console.WriteLine("Fuck.");
        // // Dispose of unmanaged resources.
        // Dispose(true);
        // // Suppress finalization.
        // GC.SuppressFinalize(this);
        // }

        /// <summary>
        ///  Enqueues a new message to be processed asynchronously
        /// </summary>
        /// <parameter name="message">The json-rpc message representing the method call to execute</parameter>
        /// <parameter name="state">
        ///  A state identifier that will be passed to the SendAsync method when a response is sent.
        /// </parameter>
        public void Dispatch(string message, string state = null) {
            _Queue.Enqueue((message, state));
        }

        /// <summary>
        ///  Marks the queue of rpc message to dispatch as complete (i.e. there will be no other 
        ///  messages dispatched).
        /// </summary>
        public void Complete() {
            _Queue.Complete();
        }

        /// <summary>
        ///  Awaits the Dispatched json-rpc messages queue until work is enqueued, then takes
        ///  those messages and converts them to JsonRpcMessage objects.
        ///
        ///  Appends the state ID onto the request Id if a response is needed, so we can use it
        ///  later on in the SendAsync method
        /// </summary>
        /// <parameter name="cancellationToken">A token to cancel the queue awaiter</parameter>
        public async ValueTask<JsonRpcMessage> ReadAsync(CancellationToken cancellationToken) {
            while(
                !cancellationToken.IsCancellationRequested &&
                (!_Queue.IsCompleted || (_Queue.IsCompleted && !_Queue.IsEmpty))
            ) {
                try {
                    var (message, state) = await _Queue.DequeueAsync(cancellationToken);

                    var bytes = Encoding.UTF8.GetBytes(message);
                    var sequence = new ReadOnlySequence<byte>(bytes);
                    var jsonRpcMessage = Formatter.Deserialize(sequence);

                    // Append state onto the Request Id if it's a request so the appropraite channel can be identified
                    // on response
                    if (jsonRpcMessage is JsonRpcRequest request && request.IsResponseExpected) {
                        request.RequestId = new RequestId($"{state}::{request.RequestId.ToString()}");
                    }

                    return jsonRpcMessage;
                } catch (OperationCanceledException) {
                    break;
                }
            }

            return null;
        }

        /// <summary>
        ///  Serializes a JsonRpcMessage to a json-rpc string and hands it off to the SendAsync delegate
        ///
        ///  If this message is a JsonRpcResult, pull the State identifier back off the Request Id and
        ///  Include it in what we hand to the delegate.  Reset the RequestID back to what it was originally.
        /// </summary>
        /// <parameter name="jsonRpcMessage">The JsonRpcMessage object to serialize and send</parameter>
        /// <parameter name="cancellationToken">A token to abort processing</parameter>
        public async ValueTask WriteAsync(JsonRpcMessage content, CancellationToken cancellationToken) {
            string state = null;

            // Extract and strip Client Id from the result if appropriate
            if (content is JsonRpcResult result && !result.RequestId.IsEmpty) {
                var splitContent = result.RequestId.ToString().Split("::", 2);
                state = splitContent[0];
                result.RequestId = ParseRequestId(splitContent[1]);
            }

            // I believe there is a cleaner way to do this with .Serialize()
            // will experiment with it later
            var pipe = new Pipe();
            this.Formatter.Serialize(pipe.Writer, content);
            await pipe.Writer.CompleteAsync();
            var stream = pipe.Reader.AsStream();
            var reader = new StreamReader(stream);
            var stringResult = await reader.ReadToEndAsync();

            // Send the message!
            await SendAsync(stringResult, state);
        }

        /// <summary>
        ///  Parses a RequestId from an object
        /// </summary>
        /// <parameter name="value">The object to parse</parameter>
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