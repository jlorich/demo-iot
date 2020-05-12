using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices.Provisioning.Service;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace MicrosoftSolutions.IoT.Demos.Service.Functions {
    public class RegisterDeviceOverHttp {
        public RegisterDeviceOverHttp() {

        }

        [FunctionName("RegisterSymmetricKeyDeviceOverHttp")]
        public async Task<IActionResult> RegisterSymmetricKeyDeviceOverHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "RegisterSymmetricKeyDeviceOverHttp")] HttpRequest req,
            ILogger log)
        {
            await Task.CompletedTask;
            // var request = await JsonSerializer.DeserializeAsync<DeviceRegistrationRequest>(req.Body);
            // var attestation = new SymmetricKeyAttestation(request.PrimaryKey, request.SecondaryKey);
            
            // var individualEnrollment = new IndividualEnrollment(
            //     request.RegistrationId,
            //     attestation
            // );

            // var client = new ProvisioningServiceClient.CreateFromConnectionString(
            //      "HostName=jl-demo-dps-dev.azure-devices-provisioning.net;SharedAccessKeyName=provisioningserviceowner;SharedAccessKey=qsBFTA4UeD6IP/gXAmh0ByW9bkGg/rrqg/5IKTIlIKs="
            // );

            // var individualEnrollmentResult = await client.CreateOrUpdateIndividualEnrollmentAsync(individualEnrollment);

            return new OkResult();
        }

    }
}




