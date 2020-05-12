using System;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftSolutions.IoT.Demos.Rpc;

[assembly: FunctionsStartup(typeof(MicrosoftSolutions.IoT.Demos.Service.Startup))]

namespace MicrosoftSolutions.IoT.Demos.Service
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register our IoT Service Client
            builder.Services.AddTransient<ServiceClient>((serviceProvider) => {
                var connectionString = Environment.GetEnvironmentVariable("IoTHubServiceConnectionString", EnvironmentVariableTarget.Process);
                return ServiceClient.CreateFromConnectionString(connectionString);
            });

            // Register our RPC client to handle sending responses
            builder.Services.AddTransient<RpcClient>((serviceProvider) => {
                var serviceClient = serviceProvider.GetService<ServiceClient>();

                return new RpcClient(async (messageString, destinationClientId) => {
                    var iotMessage = new Message(Encoding.ASCII.GetBytes(messageString));
                    await serviceClient.SendAsync(destinationClientId, iotMessage);
                });
            });
        }
    }
}