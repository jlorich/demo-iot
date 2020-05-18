using System;
using System.Text;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Rpc;
using MicrosoftSolutions.IoT.Demos.Service.Services;
using StreamJsonRpc;

[assembly: FunctionsStartup(typeof(MicrosoftSolutions.IoT.Demos.Service.Startup))]

namespace MicrosoftSolutions.IoT.Demos.Service
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddTransient<ISampleService, SampleService>();
            
            
            // Register our IoT Service Client
            builder.Services.AddSingleton<ServiceClient>((serviceProvider) => {
                var connectionString = Environment.GetEnvironmentVariable("IoTHubServiceConnectionString", EnvironmentVariableTarget.Process);
                return ServiceClient.CreateFromConnectionString(connectionString);
            });

            builder.Services.AddSingleton<DispatchingClientMessageHandler>((services) => {
                var serviceClient = services.GetService<ServiceClient>();

            });

            builder.Services.AddSingleton<JsonRpc>();

            builder.Services.AddSingleton<ISampleService>((services) => {
                var jsonRpc = services.GetService<JsonRpc>();
                var service = new SampleService();
                
                jsonRpc.AddLocalRpcTarget(service);

                return service;
            });
        }
    }
}