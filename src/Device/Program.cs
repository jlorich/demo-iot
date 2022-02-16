using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftSolutions.IoT.Demos.Common.Models;
using ProtoBuf;

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
            var rnd = new Random();
            var protoBatch = new ProtoMessageBatch() {
                Messages = new ProtoMessage[50]
            };

            while(true) {
                // Generate proto batch
                for(var j = 0; j < 50; j++) {
                    protoBatch.Messages[j] = new ProtoMessage() {
                        Tag = "TEMPERATURE",
                        Value = rnd.Next()*100,
                        Timestamp = DateTime.Now
                    };
                }
                MemoryStream stream = new MemoryStream();

                // bool once = false;
                // if (!once) {
                //     using(FileStream fs = File.Create("data.bin")) {
                //         Serializer.Serialize(fs, protoBatch);
                //     }

                //     once = true;
                // }

                Serializer.Serialize(stream, protoBatch);

                var message = new Message(stream) {
                    ContentType = "application/protobuf"
                };
                
                // read stream content and write b64 to console
                StreamReader reader = new StreamReader(message.BodyStream);
                var bytes = stream.ToArray();
                var byteString = Convert.ToBase64String(bytes);
                Console.WriteLine(byteString);

                // reset stream position
                message.BodyStream.Position = 0;
               
                await deviceClient.SendEventAsync(message);
                
                await Task.Delay(5000);
            }
        }
    }
}