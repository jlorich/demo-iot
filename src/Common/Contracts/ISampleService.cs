using System;
using System.Threading.Tasks;
using MicrosoftSolutions.IoT.Demos.Common.Models;

namespace MicrosoftSolutions.IoT.Demos.Common.Contracts {
    public interface ISampleService {
        Task<User> GetUserById(Guid userId);
        
        Task CreateUserEvent(Guid userId, UserActions action);
    }
}
