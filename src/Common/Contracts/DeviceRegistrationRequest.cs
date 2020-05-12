namespace MicrosoftSolutions.IoT.Demos.Common.Contracts {
    public class DeviceRegistrationRequest {
        public string RegistrationId { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
    }
}