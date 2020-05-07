using MicrosoftSolutions.IoT.Demos.Common.Enumerations;

namespace MicrosoftSolutions.IoT.Demos.Common.Contracts {
    public class SampleTelemetryMessage {
        public SampleUser User { get; set; }

        public SampleUserActions Action { get; set; }
    }
}