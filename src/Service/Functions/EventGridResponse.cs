using System;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Logging;
using MicrosoftSolutions.IoT.Demos.Rpc;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Common.Models;
using Newtonsoft.Json.Linq;

namespace MicrosoftSolutions.IoT.Demos.Service.Functions {
    public class EventGridResponseFunction {
        private RpcClient _rpcClient;

        public EventGridResponseFunction(RpcClient rpcClient) {
            _rpcClient = rpcClient;
        }

        [FunctionName("EventGridResponse")]
        public async Task EventGridResponse([EventGridTrigger]EventGridEvent response, ILogger log) {
            await _rpcClient.Respond<UserGetRequest>(GetBodyFromEventGridResponse(response), async (request) => {
                await Task.CompletedTask;

                // Replace this with a call to SQL
                return new UserGetResponse() {
                    User = new User {
                        FirstName = "Joe",
                        LastName = "Exotic"
                    }
                };
            });
        }

        private string GetBodyFromEventGridResponse(EventGridEvent response) {
            var base64String = ((JObject)response.Data)["body"].ToString();
            var byteArray = Convert.FromBase64String(base64String);
            var body = Encoding.UTF8.GetString(byteArray);

            return body;
        }
    }
}
