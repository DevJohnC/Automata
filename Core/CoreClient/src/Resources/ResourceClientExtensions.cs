using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.Kinds;

namespace Automata.Client.Resources
{
    public static class ResourceClientExtensions
    {
        public static IAsyncEnumerable<ResourceDocument<T>> GetResources<T>(
            this IResourceClient client)
            where T : notnull, Record
        {
            return Impl();
            
            async IAsyncEnumerable<ResourceDocument<T>> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default
                )
            {
                var kind = KindModel.GetKind(typeof(T));
                await foreach (var resource in client.GetResources(kind.Name)
                    .WithCancellation(cancellationToken))
                {
                    yield return resource.Resource.Deserialize<T>();
                }
            }
        }
    }
}