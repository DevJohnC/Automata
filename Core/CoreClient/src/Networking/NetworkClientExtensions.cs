using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Automata.Kinds;

namespace Automata.Client.Networking
{
    public static class NetworkClientExtensions
    {
        public static async IAsyncEnumerable<ResourceDocument<T>> GetResources<T>(
            this INetworkClient client,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where T : notnull, Record
        {
            var kind = KindModel.GetKind(typeof(T));
            await foreach (var resource in client.GetResources(kind.Name, cancellationToken))
            {
                yield return resource.Deserialize<T>();
            }
        }
    }
}