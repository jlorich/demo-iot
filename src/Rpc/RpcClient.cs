using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks; 

namespace MicrosoftSolutions.IoT.Demos.Rpc {
    public class MessageReceivedEventArgs : EventArgs
    {
        public string MessageType { get; set; }
        public object Message { get; set; }
    }

    public class RpcClient {
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;
            
        private static int MESSAGE_TIMEOUT_DURATION_MS = 30000;

        private Func<string, string, Task> _SendMessage;

        private static readonly SemaphoreSlim _MessageSendSemaphore = new SemaphoreSlim(1,1);

        private string _ClientId;

        // Messages pending a response
        private ConcurrentDictionary<Guid, TaskCompletionSource<object>> _Pending;


        public RpcClient(Func<string, string, Task> send) {
            _ClientId = Guid.NewGuid().ToString();
            _SendMessage = send;
            _Pending = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();
        }

        public RpcClient(string clientId, Func<string, string, Task> send) {
            _ClientId = clientId;
            _SendMessage = send;
            _Pending = new ConcurrentDictionary<Guid, TaskCompletionSource<object>>();
        }


        /// <summary>
        /// Sends a message without waiting for a response
        /// </summary>
        public void Send(object message, string destinationClientId = null, int timeout = Timeout.Infinite) {
            SendAsync(message, destinationClientId).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a message without waiting for a response
        /// </summary>
        public async Task SendAsync(object message, string destinationClientId = null, int timeout = Timeout.Infinite) {
            await SendMessageAsync(message, destinationClientId);
        }

        /// <summary>
        /// Sends a message and waits for a response
        /// </summary>
        public TResponse Call<TResponse>(object message, int timeout = Timeout.Infinite) {
            return CallAsync<TResponse>(message, timeout).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a message and waits for a response
        /// </summary>
        public async Task<TResponse> CallAsync<TResponse>(object message, int timeout = Timeout.Infinite) {
            TResponse response;

            // Create a task completion source so we can continue this method when the response is received
            var taskCompletionSource = new TaskCompletionSource<object>();

            var messageId = await SendMessageAsync(message);

            // Ensure we throw an exception after a certain time if the response isn't recieved
            timeout = timeout == Timeout.Infinite ? MESSAGE_TIMEOUT_DURATION_MS : timeout;
            var timeoutCancellationTokenSource = new CancellationTokenSource(timeout);

            timeoutCancellationTokenSource.Token.Register(
                () => taskCompletionSource.TrySetException(new TimeoutException("No message response received")),
                useSynchronizationContext: false
            );

            // Wait to receive the message
            try {
                // Record our continuation taskCompletionSource by message ID
                // this will be used to trigger the continuation below
                _Pending[messageId] = taskCompletionSource;

                var contentByteString = (string)(await taskCompletionSource.Task.ConfigureAwait(false));
                var bytes = Convert.FromBase64String(contentByteString);
                var contentString = Encoding.UTF8.GetString(bytes);
                response = JsonSerializer.Deserialize<TResponse>(contentString);

                timeoutCancellationTokenSource.Dispose();                 

            } finally {
                _Pending.TryRemove(messageId, out TaskCompletionSource<object> item);
            }
            
            return (TResponse)response;
        }

        /// <summary>
        /// Puts a new message response on the work queue to be processed
        /// Resumes the work if worker is idle.
        /// </summary>
        public void Dispatch(string rawMessage) {
            var message = JsonSerializer.Deserialize<RpcMessage>(rawMessage);
            
            if (_Pending.ContainsKey(message.Id)) {
                _Pending[message.Id].SetResult(message.Content);
            } else {
                var args = new MessageReceivedEventArgs() {
                    MessageType = message.Type,
                    Message = message
                };

                MessageReceived?.Invoke(this, args);
            }
        }

        /// <summary>
        /// Responds to a given message
        /// </summary>
        public async Task Respond<TRequest>(string rawMessage, Func<TRequest, Task<object>> handler) {
            var message = JsonSerializer.Deserialize<RpcMessage>(rawMessage);

            if (message.Type != typeof(TRequest).Name) {
                throw new Exception("Invalid type sepcified");
            }
            
            var bytes = Convert.FromBase64String(message.Content);
            var contentString = Encoding.UTF8.GetString(bytes);
            var content = JsonSerializer.Deserialize<TRequest>(contentString);
            var responseContent = await handler(content);

            await SendMessageAsync(responseContent, message.ClientId, message.Id);
        }

        // Serializes and sends a message in a thread-safe manner
        private async Task<Guid> SendMessageAsync(object content, string destinationClientId = null, Guid messageId = new Guid()) {
            // Moving this to base64 fixes a lot of string quote deserialization issues
            var contentString = JsonSerializer.Serialize(content);
            var contentBytes = Encoding.UTF8.GetBytes(contentString);
            var contentBytesString = Convert.ToBase64String(contentBytes);

            var rpcMessage = new RpcMessage() {
                Type = content.GetType().Name,
                Version = content.GetType().Assembly.GetName().Version,
                Id = messageId == Guid.Empty ? Guid.NewGuid() : messageId,
                ClientId = _ClientId,
                Content = contentBytesString
            };

            // Allow nested object serialization
            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.MaxDepth = 100;

            var messageString = JsonSerializer.Serialize(rpcMessage, serializerOptions);

            try {
                await _MessageSendSemaphore.WaitAsync();
                await _SendMessage(messageString, destinationClientId).ConfigureAwait(false);
            } finally {
                _MessageSendSemaphore.Release();
            }

            return rpcMessage.Id;
        }
    }
}