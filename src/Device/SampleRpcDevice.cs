using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Common.Models;

namespace MicrosoftSolutions.IoT.Demos.Device {
    public class SampleRpcDevice {
        private ISampleService _SampleService;
        private DeviceClient _Client;

        public SampleRpcDevice(DeviceClient client, ISampleService service) {
            _Client = client;
            _SampleService = service;

        }

        // Sends a simple telemetry message to an IoT DeviceClient
        public async Task CreateUserEvent() {
            await _SampleService.CreateUserEvent(Guid.NewGuid(), UserActions.KILLED_HER_HUSBAND);
        }

        // Sends a request/response message using a message dispatcher
        public async Task<User> GetUser(Guid userid) {
            return await _SampleService.GetUserById(userid);
        }
    }
}