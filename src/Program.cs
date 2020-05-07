using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftSolutions.IoT.Demos.Common;
using MicrosoftSolutions.IoT.Demos.DeviceAuthenticationFactories;
using MicrosoftSolutions.IoT.Demos.DeviceClientFactories;
using MicrosoftSolutions.IoT.Demos.DeviceRegistrationProviders;
using MicrosoftSolutions.IoT.Demos.SecurityProviderFactories;
using MicrosoftSolutions.IoT.Demos.Options;

namespace MicrosoftSolutions.IoT.Demos
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

            // Don't use DPS:
            //services.AddTransient<IDeviceRegistrationProvider, DeviceRegistrationSimpleProvider>();
            //services.Configure<DeviceRegistrationSimpleOptions>(config.GetSection("simpleRegistration"));

            // Use DPS:
            services.AddTransient<IDeviceRegistrationProvider, DeviceRegistrationDpsProvider>();
            services.Configure<DeviceRegistrationDpsOptions>(config.GetSection("dpsRegistration"));


            services.AddTransient<IDeviceClientFactory, DeviceClientFactory>();

            services.AddTransient<SecurityProvider>((services) => {
                return services.GetService<ISecurityProviderFactory>().Create();
            });

            services.AddTransient<IAuthenticationMethod>((services) => {
                return services.GetService<IDeviceAuthenticationFactory>().Create().GetAwaiter().GetResult();
            });

            services.AddTransient<DeviceClient>((services) => {
                return services.GetService<IDeviceClientFactory>().Create().GetAwaiter().GetResult();
            });

            services.AddTransient<SampleDevice>();
        }

        private static async Task Run() {
            var provider = services.BuildServiceProvider();
            
            var device = provider.GetService<SampleDevice>();
            await device.SendTelemetryMessage();
        }       
    }
}