using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;

namespace MicrosoftSolutions.IoT.Demos.Device.SecurityProviderFactories {

    public interface ISecurityProviderFactory {
        SecurityProvider Create();
    }
}