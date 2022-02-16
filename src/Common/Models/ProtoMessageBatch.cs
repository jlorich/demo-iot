using System;
using ProtoBuf;

namespace MicrosoftSolutions.IoT.Demos.Common.Models {
    [ProtoContract]
    public class ProtoMessageBatch {
        [ProtoMember(1)]
        public ProtoMessage[] Messages {get;set;}
    }
}