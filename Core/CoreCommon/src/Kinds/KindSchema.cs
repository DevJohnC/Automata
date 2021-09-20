using System;
using System.IO;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Swashbuckle.AspNetCore.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Automata.Kinds
{
    [JsonConverter(typeof(KindSchemaJsonConverter))]
	public sealed record KindSchema
	{
		private readonly SchemaRepository _schemaRepository;

		private readonly string _rootSchema;

		public KindSchema(SchemaRepository schemaRepository, string rootSchema, bool isAbstract)
		{
			_schemaRepository = schemaRepository;
			_rootSchema = rootSchema;
			IsAbstract = isAbstract;
		}

		public bool IsAbstract { get; }

		public static KindSchema CreateFromType(Type type, bool isAbstract = false)
		{
			var jsonSettings = new JsonSerializerSettings();
			jsonSettings.Converters.Add(new StringEnumConverter());
			
			var openApiGenerator = new SchemaGenerator(
				new SchemaGeneratorOptions(),
				new NewtonsoftDataContractResolver(jsonSettings));

			var repository = new SchemaRepository();
			var openApiSchema = openApiGenerator.GenerateSchema(type, repository);

			return new KindSchema(repository, openApiSchema.Reference?.ReferenceV3 ?? string.Empty, isAbstract);
		}

		private class KindSchemaJsonConverter : JsonConverter<KindSchema>
		{
			public override KindSchema ReadJson(JsonReader reader, Type objectType,
				KindSchema? existingValue, bool hasExistingValue,
				JsonSerializer serializer)
			{
				if (serializer.Deserialize(reader) is not JObject next)
					throw new Exception();

				var jobjSchemas = next.Value<JObject>("components")
					.Value<JObject>("schemas");

				var repository = new SchemaRepository();
				var openApiReader = new OpenApiStringReader();
				foreach (JProperty schemaProperty in jobjSchemas.Children())
				{
					var schemaJson = schemaProperty.Value.ToString();
					var schema = openApiReader.ReadFragment<OpenApiSchema>(
						schemaJson, Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0, out var _);

					repository.Schemas.Add(
						schemaProperty.Name,
						schema
						);
				}

				return new KindSchema(repository, next.Value<string>("root"), next.Value<bool>("isAbstract"));
			}

			public override void WriteJson(JsonWriter writer, KindSchema? value, JsonSerializer serializer)
			{
				if (value is null)
				{
					writer.WriteNull();
					return;
				}

				writer.WriteStartObject();
				writer.WritePropertyName("root");
				writer.WriteValue(value._rootSchema);
				writer.WritePropertyName("isAbstract");
				writer.WriteValue(value.IsAbstract);
				writer.WritePropertyName("components");
				writer.WriteStartObject();
				writer.WritePropertyName("schemas");
				writer.WriteStartObject();
				foreach (var schema in value._schemaRepository.Schemas)
				{
					writer.WritePropertyName(schema.Key);

					using var stringWriter = new StringWriter();
					var jsonWriter = new OpenApiJsonWriter(stringWriter);
					schema.Value.SerializeAsV3(jsonWriter);
					var str = stringWriter.ToString();

					serializer.Serialize(writer, JToken.Parse(str));
				}
				writer.WriteEndObject();
				writer.WriteEndObject();
				writer.WriteEndObject();
			}
		}
	}
}