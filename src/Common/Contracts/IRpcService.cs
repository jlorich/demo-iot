using System;
using System.Threading.Tasks;
using MicrosoftSolutions.IoT.Demos.Common.Models;

namespace MicrosoftSolutions.IoT.Demos.Common.Contracts {
    public interface IRpcService {
        Task<User> GetUserById(Guid userId);
        
        Task CreateUserEvent(Guid userId, UserAction action);
    }
}
