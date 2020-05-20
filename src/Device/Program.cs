using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Common.Models;
using MicrosoftSolutions.IoT.Demos.Device.DeviceAuthenticationFactories;
using MicrosoftSolutions.IoT.Demos.Device.DeviceClientFactories;
using MicrosoftSolutions.IoT.Demos.Device.DeviceRegistrationProviders;
using MicrosoftSolutions.IoT.Demos.Device.Options;
using MicrosoftSolutions.IoT.Demos.Device.SecurityProviderFactories;
using MicrosoftSolutions.IoT.Demos.Rpc;
using StreamJsonRpc;

namespace MicrosoftSolutions.IoT.Demos.Device
{

    public class Program
    {
        private static IServiceCollection _Services;
        private static IConfigurationRoot _Config;


        // Main program method
        private async static Task Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("local.settings.json");
            
            _Config = configurationBuilder.Build();
            _Services = new ServiceCollection();

            var startup = new Startup(_Config);
            startup.ConfigureServices(_Services);

            await RunAsync();
        }

        private static async Task RunAsync() {
            var provider = _Services.BuildServiceProvider();
            var deviceClient = provider.GetService<DeviceClient>();
            var rpcService = provider.GetService<IRpcService>();

            while(true) {
                Console.WriteLine("--- Sending telemetry message ---");
                var userId = Guid.NewGuid();

                var user = await rpcService.GetUserById(userId);

                await rpcService.CreateUserEvent(
                    user.Id,
                    UserAction.KilledHerHusband
                );

                Console.WriteLine("--- Getting User ---");
                

                Console.WriteLine(JsonSerializer.Serialize(user));

                // await device.UploadTestFileToStorage();
                await Task.Delay(5000);
            }
        }       
    }
}