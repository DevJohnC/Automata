using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Automata.GrpcServices
{
    public partial class ResourceRecord
    {
        public SerializedResourceDocument ToNative()
        {
            return new SerializedResourceDocument(
                Guid.Parse(ResourceId),
                KindUri.NativeKindUri,
                JObject.Parse(RecordData.EncodedBlob));
        }
        
        public static ResourceRecord FromNative(SerializedResourceDocument serializedResourceDocument)
        {
            return new ResourceRecord()
            {
                ResourceId = serializedResourceDocument.ResourceId.ToString(),
                KindUri = new()
                {
                    Group = serializedResourceDocument.KindUri.Group,
                    KindNameSingular = serializedResourceDocument.KindUri.Name,
                    Version = serializedResourceDocument.KindUri.Version
                },
                RecordData = new()
                {
                    EncodedBlob = serializedResourceDocument.Record.ToString(Formatting.None)
                }
            };
        }
    }
}