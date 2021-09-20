using System;
using System.IO;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Automata.Json
{
    public class OpenApiSchemaConverter : JsonConverter<OpenApiSchema>
    {
        public override void WriteJson(JsonWriter writer, OpenApiSchema value, JsonSerializer serializer)
        {
            using var stringWriter = new StringWriter();
            var jsonWriter = new OpenApiJsonWriter(stringWriter);
            value.SerializeAsV3WithoutReference(jsonWriter);
            var str = stringWriter.ToString();
            //  re-serialize to make sure the json matches the serializer settings
            serializer.Serialize(writer, JObject.Parse(str));
        }

        public override OpenApiSchema ReadJson(JsonReader reader, Type objectType, OpenApiSchema existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var next = serializer.Deserialize(reader);
            if (next == null)
                return new OpenApiSchema();
            var openApiReader = new OpenApiStringReader();
            var schema = openApiReader.ReadFragment<OpenApiSchema>(next.ToString(),
                Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0, out var _);
            return schema;
        }
    }
}