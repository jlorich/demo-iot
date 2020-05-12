// using System;
// using System.Text.Json;
// using System.Text.Json.Serialization;

// namespace MicrosoftSolutions.IoT.Demos.Rpc
// {
//     public class RpcMessageConverter<T> : JsonConverter<RpcMessage>
//     {
//         public override RpcMessage Read(
//             ref Utf8JsonReader reader,
//             Type typeToConvert,
//             JsonSerializerOptions options
//         ) {
//             var document = JsonDocument.ParseValue(ref reader);
//             var root = document.RootElement;

//             if (typeof(string).Equals(typeof(T)) || root.GetProperty("Type").GetString() != typeof(T).Name) {
//                 throw new Exception("IncompaibleTypes");
//             }
            
//             var id = root.GetProperty("Id").GetGuid();
//             var clientId = root.GetProperty("ClientId").GetString();
//             var version = Version.Parse(root.GetProperty("Version").GetString());
//             var content = root.GetProperty("Content").ToString(); 

//             return new RpcMessage() {
//                 Type = typeof(T),
//                 Id = id,
//                 ClientId = clientId,
//                 Version = version,
//                 Content = content
//             };
//         }

//         public override void Write(
//             Utf8JsonWriter writer,
//             RpcMessage message,
//             JsonSerializerOptions options
//         ) {
//             throw new NotImplementedException();
//         }
//     }
// }