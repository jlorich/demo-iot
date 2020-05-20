using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MicrosoftSolutions.IoT.Demos.Common.Contracts;
using MicrosoftSolutions.IoT.Demos.Common.Models;

namespace MicrosoftSolutions.IoT.Demos.Service.Services {

    /// <summary>
    ///  This class is a sample RPC services that will be invoked by the StreamJsonRpc library
    /// </summary>
    public class RpcService : IRpcService {

        /// <summary>
        /// Constructs a new RpcService
        /// </summary>
        public RpcService() {
            
        }
        
        /// <summary>
        /// Gets a users information by their user Id
        /// </summary>
        /// <parameter name="userId">The id of the user to get</parameter>
        public async Task<User> GetUserById(Guid userId) {
            await Task.CompletedTask;

            return new User {
                Id = userId,
                FirstName = "Carol",
                LastName = "Baskin"
            };
        }
        
        /// <summary>
        /// Records a new user event
        /// </summary>
        /// <parameter name="userId">The id of the user to create the event for</parameter>
        /// <parameter name="action">The action the user performed</parameter>
        public async Task CreateUserEvent(Guid userId, UserAction action) {
            await Task.CompletedTask;
        }
    }
}
