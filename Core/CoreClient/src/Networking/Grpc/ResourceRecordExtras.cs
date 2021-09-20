using System;
using Newtonsoft.Json.Linq;

namespace Automata.GrpcServices
{
    public partial class ResourceRecord
    {
        public SerializedResourceDocument ToResourceDocument()
        {
            return new SerializedResourceDocument(
                Guid.Parse(ResourceId),
                Kinds.KindUri.Singular(
                    KindUri.Group,
                    KindUri.Version,
                    KindUri.KindNameSingular),
                JObject.Parse(
                    RecordData.EncodedBlob));
        }
    }
}