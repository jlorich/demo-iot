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

            // Regiser our ServiceClient to call out to our services
            services.AddHttpClient<DeviceEnrollmentHttpService>(client => {
                client.BaseAddress = new Uri("http://localhost:7071/api/");
            });

            // Register our RPC client to send RPC messages over MQTT/AMQP
            services.AddSingleton<RpcClient>((serviceProvider) => {
                var deviceClient = serviceProvider.GetService<DeviceClient>();
                var deviceRegistrationProvider = serviceProvider.GetService<IDeviceRegistrationProvider>();
                var deviceId = deviceRegistrationProvider.DeviceId;

                return new RpcClient(deviceId, async (messageString, destinationClientId) => {
                    var iotMessage = new Message(Encoding.ASCII.GetBytes(messageString));
                    await deviceClient.SendEventAsync(iotMessage);
                });
            });

            // Add the sample device
            services.AddTransient<SampleDevice>();
        }

        private static async Task Run() {
            var provider = services.BuildServiceProvider();

            var registrationService = provider.GetService<DeviceEnrollmentHttpService>();
            //await registrationService.Enroll();

            var device = provider.GetService<SampleDevice>();
            

            while(true) {
                Console.WriteLine("Sending telemetry message");

                var user = await device.GetUser(new UserGetRequest() {
                    UserId = Guid.NewGuid()
                });

                Console.WriteLine(JsonSerializer.Serialize(user));

                await device.UploadTestFileToStorage();
                await Task.Delay(5000);
            }
        }       
    }
}