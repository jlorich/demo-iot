using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Threading;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Device.DeviceAuthenticationFactories;
using MicrosoftSolutions.IoT.Demos.Device.DeviceClientFactories;
using MicrosoftSolutions.IoT.Demos.Device.DeviceRegistrationProviders;
using MicrosoftSolutions.IoT.Demos.Device.Options;
using MicrosoftSolutions.IoT.Demos.Device.SecurityProviderFactories;
using MicrosoftSolutions.IoT.Demos.Rpc;
using StreamJsonRpc;

namespace MicrosoftSolutions.IoT.Demos.Device
{
    /// <summary>
    ///  This class manages DI service registration for this IoT Device
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }

        private ConfigurationOptions _ConfigurationOptions;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _ConfigurationOptions = Configuration.Get<ConfigurationOptions>();
        }

        /// <summary>
        ///  Register appropriate services for dependency injection, such as things for
        ///  logging, configuration authentication, registration, etc.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            // Log console message
            services.AddLogging(builder => builder
                .AddConsole()
                .AddFilter(level => level >= _ConfigurationOptions.LogLevel)
            );

            ConfigureAuthenticationServices(services);
            ConfigureRegistrationServices(services);
            ConfigureDeviceServices(services);
            ConfigureRpcServices(services);
        }

        /// <summary>
        ///  Registers authentication-related services with the Dependency Injection provider
        /// </summary>
        private void ConfigureAuthenticationServices(IServiceCollection services) {
            // Register the desired registration method
            switch (_ConfigurationOptions.AuthenticationMethod) {
                case AuthenticationMethod.SymmetricKey:
                    services.AddTransient<IDeviceAuthenticationFactory, DeviceAuthenticationSymmetricKeyFactory>();
                    services.AddTransient<ISecurityProviderFactory, SecurityProviderSymmetricKeyFactory>();
                    services.Configure<SecurityProviderSymmetricKeyOptions>(Configuration.GetSection("symmetricKeySecurity"));
                    break;
                case AuthenticationMethod.X509:
                    services.AddTransient<IDeviceAuthenticationFactory, DeviceAuthenticationX509Factory>();
                    services.AddTransient<ISecurityProviderFactory, SecurityProviderX509Factory>();
                    services.Configure<SecurityProviderX509Options>(Configuration.GetSection("x509Security"));
                    break;
            }

            // Register a SecurityProvider Instance generated by the relevant Security Provider Factory
            services.AddTransient<SecurityProvider>((services) => {
                return services.GetService<ISecurityProviderFactory>().Create();
            });

            // Register an IAuthenticationMethod Instance generated by the relevant IDeviceAuthenticationFactory
            services.AddTransient<IAuthenticationMethod>((services) => {
                return services.GetService<IDeviceAuthenticationFactory>().Create();
            });
        }

        /// <summary>
        ///  Registers device registration related services with the Dependency Injection provider
        /// </summary>
        private void ConfigureRegistrationServices(IServiceCollection services) {
            switch (_ConfigurationOptions.RegistrationMethod) {
                case RegistrationMethod.Simple:
                    services.AddSingleton<IDeviceRegistrationProvider, DeviceRegistrationSimpleProvider>();
                    services.Configure<DeviceRegistrationSimpleOptions>(Configuration.GetSection("simpleRegistration"));
                    break;

                case RegistrationMethod.DeviceProvisioningService:
                    services.AddSingleton<IDeviceRegistrationProvider>((services) => {
                        var provider = new DeviceRegistrationDpsProvider(
                            services.GetService<IOptions<DeviceRegistrationDpsOptions>>(),
                            services.GetService<IOptions<TransportOptions>>(),
                            services.GetService<SecurityProvider>()
                        );

                        // Safely await our provider registration
                        var jtf = new JoinableTaskFactory(new JoinableTaskContext());

                        jtf.Run(async delegate
                        {
                            await provider.RegisterAsync();
                        });

                        return provider;
                    });

                    services.Configure<DeviceRegistrationDpsOptions>(Configuration.GetSection("dpsRegistration"));
                    break;
            }
        }

        /// <summary>
        ///  Registers devices-related services with the Dependency Injection provider
        /// </summary>
        private void ConfigureDeviceServices(IServiceCollection services) {
            // Register the relevatne DeviceClientFactory
            services.AddTransient<IDeviceClientFactory, DeviceClientFactory>();
            
            // Register a DeviceClient instance generated by 
            services.AddSingleton<DeviceClient>((services) => {
                return services.GetService<IDeviceClientFactory>().Create();
            });

            services.Configure<TransportOptions>(Configuration.GetSection("transport"));
        }

        /// <summary>
        ///  Registers Rpc-related services with the Dependency Injection provider
        /// </summary>
        private void ConfigureRpcServices(IServiceCollection services) {
            // Register our RPC services and tie it to a sample service
            services.AddSingleton<IRpcService>((services) => {
                var deviceClient = services.GetService<DeviceClient>();
                var registration = services.GetService<IDeviceRegistrationProvider>();
                
                var handler = new DispatchingClientMessageHandler();
                var jsonRpc = new JsonRpc(handler);

                // Receive
                deviceClient.SetMethodHandlerAsync(_ConfigurationOptions.RpcMethodName, (request, context) => {
                    handler.Dispatch(request.DataAsJson, registration.DeviceId);
                    return Task.FromResult(new MethodResponse(new byte[0], 200));
                }, null).ConfigureAwait(false).GetAwaiter().GetResult();

                // Send
                handler.SendAsync = async (message, clientId) => {
                    var iotMessage = new Message(Encoding.UTF8.GetBytes(message));
                    await deviceClient.SendEventAsync(iotMessage);
                };

                jsonRpc.StartListening();

                return jsonRpc.Attach<IRpcService>();
            });
        }  
    }
}