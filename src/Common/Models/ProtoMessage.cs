using System;
using ProtoBuf;

namespace MicrosoftSolutions.IoT.Demos.Common.Models {
    [ProtoContract]
    public class ProtoMessage {
        [ProtoMember(1)]
        public string Tag {get;set;}
        [ProtoMember(2)]
        public float Value {get;set;}
        [ProtoMember(3)]
        public DateTime Timestamp {get;set;}
    }
}