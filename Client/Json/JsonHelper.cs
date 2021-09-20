using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Automata.Client.Json
{
    public static class JsonHelper
    {
        public static async Task<JToken> ReadToken(Stream jsonStream,
            CancellationToken cancellationToken)
        {
            using var streamReader = new StreamReader(jsonStream);
            using var reader = new JsonTextReader(streamReader);

            return await JToken.ReadFromAsync(reader, cancellationToken);
        }
        
        public static async IAsyncEnumerable<JObject> ReadJsonArray(Stream jsonStream,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var streamReader = new StreamReader(jsonStream);
            using var reader = new JsonTextReader(streamReader);

            if (!await reader.ReadAsync(cancellationToken) ||
                reader.TokenType != JsonToken.StartArray)
            {
                throw new InvalidOperationException("Invalid JSON, stream doesn't start an array.");
            }
            
            while (await reader.ReadAsync(cancellationToken) &&
                   reader.TokenType != JsonToken.EndArray)
            {
                cancellationToken.ThrowIfCancellationRequested();

                JObject? jObject;
                if (reader.TokenType != JsonToken.StartObject ||
                    (jObject = await JToken.ReadFromAsync(reader, cancellationToken) as JObject) == null)
                {
                    continue;
                }

                yield return jObject;
            }
        }
    }
}