using System;
using System.Threading.Tasks;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Common.Models;

namespace MicrosoftSolutions.IoT.Demos.Service.Services {
    public class SampleService : ISampleService {
        public async Task<User> GetUserById(Guid userId) {
            // Go connect to sql here
            await Task.CompletedTask;

            return new User {
                FirstName = "Carol",
                LastName = "Baskin"
            };
        }
        
        public async Task CreateUserEvent(Guid userId, UserActions action) {
            // Go connect to sql here
            await Task.CompletedTask;

            Console.WriteLine("User event created!");
        }
    }
}
