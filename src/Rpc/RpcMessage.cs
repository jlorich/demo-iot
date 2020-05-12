using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicrosoftSolutions.IoT.Demos.Rpc {

    public class RpcMessage {
        public string Type { get; set; }

        [JsonConverter(typeof(VersionConverter))]
        public Version Version { get; set; }

        public Guid Id { get; set;}

        public string ClientId { get; set;}

        public string Content { get; set; }
    }
}