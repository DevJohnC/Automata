using System;
using Automata.Kinds;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Automata
{
    public record SerializedResourceDocument(Guid ResourceId, SingularKindUri KindUri, JObject Record) :
        ResourceIdentifier(ResourceId, KindUri)
    {
        public ResourceDocument<T> Deserialize<T>()
            where T : notnull, Record
        {
            return ResourceSerializer.Deserialize<T>(this);
        }
    }

    public abstract record ResourceDocument(Guid ResourceId, SingularKindUri KindUri, Record Record) :
        ResourceIdentifier(ResourceId, KindUri)
    {
        public abstract SerializedResourceDocument Serialize();
    }
    
    public record ResourceDocument<T> : ResourceDocument
        where T : notnull, Record
    {
        public new T Record { get; }
        
        public ResourceDocument(Guid resourceId, T record) :
            base(resourceId, record.GetKind().Name.SingularUri, record)
        {
            Record = record;
        }

        public override SerializedResourceDocument Serialize()
        {
            return ResourceSerializer.Serialize(this);
        }
    }
    
    //  todo: consider renaming this to something more general to the serialization needs of the API
    public static class ResourceSerializer
    {
        private static readonly JsonSerializer _jsonSerializer;

        static ResourceSerializer()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new Json.OpenApiSchemaConverter());
            _jsonSerializer = JsonSerializer.Create(settings);
        }

        public static JObject Serialize<T>(T resource) where T : Record
        {
            return JObject.FromObject(resource, _jsonSerializer);
        }

        public static SerializedResourceDocument Serialize<T>(ResourceDocument<T> resourceDocument)
            where T : notnull, Record
        {
            return new SerializedResourceDocument(
                resourceDocument.ResourceId,
                resourceDocument.KindUri,
                JObject.FromObject(resourceDocument.Record, _jsonSerializer)
            );
        }
        
        public static ResourceDocument<T> Deserialize<T>(SerializedResourceDocument serializedResourceDocument)
            where T : notnull, Record
        {
            return new ResourceDocument<T>(
                serializedResourceDocument.ResourceId,
                serializedResourceDocument.Record.ToObject<T>(_jsonSerializer)
            );
        }
    }
}