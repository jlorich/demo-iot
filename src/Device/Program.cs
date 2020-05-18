using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Device.DeviceAuthenticationFactories;
using MicrosoftSolutions.IoT.Demos.Device.DeviceClientFactories;
using MicrosoftSolutions.IoT.Demos.Device.DeviceEnrollmentServices;
using MicrosoftSolutions.IoT.Demos.Device.DeviceRegistrationProviders;
using MicrosoftSolutions.IoT.Demos.Device.Options;
using MicrosoftSolutions.IoT.Demos.Device.SecurityProviderFactories;
using MicrosoftSolutions.IoT.Demos.Rpc;
using StreamJsonRpc;

namespace MicrosoftSolutions.IoT.Demos.Device
{
    public class Program
    {
        private static IServiceCollection services;
        private static IConfigurationRoot config;

        // Main program method
        private static void Main(string[] args)
        {
            LoadConfiguration();
            ConfigureServices();
            Run().GetAwaiter().GetResult();
        }

        // Set up configuration to load local.settings.json file
        private static void LoadConfiguration() {
            
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile("local.settings.json");
            
            config = configurationBuilder.Build();
        }

        // Create a collection of registered services for dependency injection
        private static void ConfigureServices()
        {
            services = new ServiceCollection();

            // Use X509:
            services.AddTransient<IDeviceAuthenticationFactory, DeviceAuthenticationX509Factory>();
            services.AddTransient<ISecurityProviderFactory, SecurityProviderX509Factory>();
            services.Configure<SecurityProviderX509Options>(config.GetSection("x509Security"));

            // Use Symmetric Key:
            //services.AddTransient<IDeviceAuthenticationFactory, DeviceAuthenticationSymmetricKeyFactory>();
            //services.AddTransient<ISecurityProviderFactory, SecurityProviderSymmetricKeyFactory>();
            //services.Configure<SecurityProviderSymmetricKeyOptions>(config.GetSection("symmetricKeySecurity"));

            // Use DPS:
            services.AddSingleton<IDeviceRegistrationProvider, DeviceRegistrationDpsProvider>();
            services.Configure<DeviceRegistrationDpsOptions>(config.GetSection("dpsRegistration"));

            // Skip DPS:
            //services.AddTransient<IDeviceRegistrationProvider, DeviceRegistrationSimpleProvider>();
            //services.Configure<DeviceRegistrationSimpleOptions>(config.GetSection("simpleRegistration"));


            // Register the SecurityProvider Instance
            services.AddTransient<SecurityProvider>((services) => {
                return services.GetService<ISecurityProviderFactory>().Create();
            });

            // Register the AuthenticationMethod Instnace
            services.AddTransient<IAuthenticationMethod>((services) => {
                return services.GetService<IDeviceAuthenticationFactory>().Create().GetAwaiter().GetResult();
            });

            services.AddTransient<IDeviceClientFactory, DeviceClientFactory>();
            
            // Register the DeviceClient Instance
            services.AddSingleton<DeviceClient>((services) => {
                return services.GetService<IDeviceClientFactory>().Create().GetAwaiter().GetResult();
            });

            // Register our RPC services and tie it to a sample service
            services.AddSingleton<ISampleService>((services) => {
                var deviceClient = services.GetService<DeviceClient>();
                var registration = services.GetService<IDeviceRegistrationProvider>();
                
                var handler = new DispatchingClientMessageHandler();
                var jsonRpc = new JsonRpc(handler);

                // Receive
                deviceClient.SetMethodHandlerAsync("rpc", (request, context) => {
                    handler.Dispatch(registration.DeviceId, request.DataAsJson);
                    return Task.FromResult(new MethodResponse(new byte[0], 200));
                }, null).ConfigureAwait(false).GetAwaiter().GetResult();

                // Send
                handler.SendAsync = async (clientId, message) => {
                    var iotMessage = new Message(Encoding.UTF8.GetBytes(message));
                    await deviceClient.SendEventAsync(iotMessage);
                };

                jsonRpc.StartListening();

                return jsonRpc.Attach<ISampleService>();
                
            });

            // Add the sample device
            services.AddSingleton<SampleRpcDevice>();
        }

        private static async Task Run() {
            var provider = services.BuildServiceProvider();

            var registrationService = provider.GetService<DeviceEnrollmentHttpService>();
            //await registrationService.Enroll();

            var device = provider.GetService<SampleRpcDevice>();
            

            while(true) {
                Console.WriteLine("--- Sending telemetry message ---");
                await device.CreateUserEvent();

                Console.WriteLine("--- Getting User ---");
                var user = await device.GetUser(Guid.NewGuid());

                Console.WriteLine(JsonSerializer.Serialize(user));

                // await device.UploadTestFileToStorage();
                 await Task.Delay(5000);
            }
        }       
    }
}