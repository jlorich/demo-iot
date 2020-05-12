using MicrosoftSolutions.IoT.Demos.Common.Models;

namespace MicrosoftSolutions.IoT.Demos.Common.Contracts {
    public class UserActionCreateRequest {
        public User User { get; set; }

        public UserActions Action { get; set; }
    }
}