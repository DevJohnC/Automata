using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.HostServer.Infrastructure;
using Automata.Kinds;

namespace Automata.HostServer.Api
{
    public class ResourceProviderResourceApi : IResourceApi
    {
        private readonly List<IResourceProvider> _resourceProviders;

        public ResourceProviderResourceApi(IEnumerable<IResourceProvider> resourceProviders)
        {
            _resourceProviders = resourceProviders.ToList();
        }

        public IAsyncEnumerable<SerializedResourceDocument> GetResources(KindUri kindUri)
        {
            return Impl();

            async IAsyncEnumerable<SerializedResourceDocument> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                foreach (var provider in _resourceProviders)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await foreach (var resource in provider.GetResources()
                        .WithCancellation(cancellationToken))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var recordData = resource.Record;
                        if (!recordData.IsOfKind(kindUri)) continue;

                        yield return resource.Serialize();
                    }
                }
            }
        }
    }
}