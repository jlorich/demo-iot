using System;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Rpc;
using MicrosoftSolutions.IoT.Demos.Service.Services;
using StreamJsonRpc;

[assembly: FunctionsStartup(typeof(MicrosoftSolutions.IoT.Demos.Service.Startup))]

namespace MicrosoftSolutions.IoT.Demos.Service
{
    /// <summary>
    ///  This class manages DI service registration for this Azure Function App
    /// </summary>
    public class Startup : FunctionsStartup
    {
        private const string CLOUD_TO_DEVICE_METHOD_NAME = "rpc";

        /// <summary>
        ///  Configures DI Services for this Function App
        /// </summary>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register our IoT Service Client as a singleton, since it can be shared between
            // multiple functions being executed on the same process
            builder.Services.AddSingleton<ServiceClient>((serviceProvider) => {
                var connectionString = Environment.GetEnvironmentVariable("IoTHubServiceConnectionString", EnvironmentVariableTarget.Process);
                return ServiceClient.CreateFromConnectionString(connectionString);
            });

            ConfigureRpcServices(builder.Services);
        }

        /// <summary>
        ///  Registers Rpc-related services with the Dependency Injection provider
        /// </summary>
        private void ConfigureRpcServices(IServiceCollection services) {
            // Register a Dispatching RPC Message Handler scoped for each function invocation
            // Configure this handler to send outgoing messages using IoT CloudToDeviceMethods
            services.AddScoped<DispatchingClientMessageHandler>((services) => {
                var serviceClient = services.GetService<ServiceClient>();
                var logger = services.GetService<ILogger<FunctionsStartup>>();

                var handler = new DispatchingClientMessageHandler();

                handler.SendAsync = async (message, clientId) => {
                    logger.LogTrace($"Sending RPC message to {clientId}");
                    logger.LogTrace(message);
                    
                    var method = new CloudToDeviceMethod(CLOUD_TO_DEVICE_METHOD_NAME);
                    method.SetPayloadJson(message);

                    await serviceClient.InvokeDeviceMethodAsync(clientId, method);

                    logger.LogTrace($"Successfully sent RPC message to {clientId}");
                };

                return handler;
            });

            // Register a Function-scoped JsonRpc that uses the Message Handler
            services.AddScoped<JsonRpc>((services) => 
                new JsonRpc(services.GetService<DispatchingClientMessageHandler>()){
                    CancelLocallyInvokedMethodsWhenConnectionIsClosed = false
                }
            );
            
            // Register a new SampleService instance that's an invocation target for JsonRpc
            services.AddScoped<IRpcService>((services) => {
                var jsonRpc = services.GetService<JsonRpc>();
                var handler = services.GetService<DispatchingClientMessageHandler>();
                var service = new RpcService();

                jsonRpc.AddLocalRpcTarget(service);
                jsonRpc.StartListening();

                return service;
            });
        }
    }
}