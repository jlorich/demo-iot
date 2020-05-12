using System;
using System.Net;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Device.Options;
using System.Text;

namespace MicrosoftSolutions.IoT.Demos.Device.DeviceEnrollmentServices {

    // Provides device registration information from the IoT Hub Device Provisioning Service
    public class DeviceEnrollmentHttpService {
        private HttpClient _httpClient;

        public DeviceEnrollmentHttpService(HttpClient httpClient){
            _httpClient = httpClient;
        }
        
        public async Task Enroll() {
            var request = new DeviceRegistrationRequest() {
                RegistrationId = "test-device-http-enrollment",
                PrimaryKey = "ove/it6vxy1zDizVifZ6zRY6OU23YFhSLfCcOAtpyJI=",
                SecondaryKey = "aMtcOVPxRAJ5Zx0zNHDa4CMMplQKf/0+pNuYuciqPzA="
            };

            var requestString = JsonSerializer.Serialize(request);
            var stringContent = new StringContent(requestString, Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.PostAsync("RegisterSymmetricKeyDeviceOverHttp", stringContent);
            var responseStream = await httpResponse.Content.ReadAsStreamAsync();
            var response = await JsonSerializer.DeserializeAsync<DeviceRegistrationResponse>(responseStream);
        }
    }
}