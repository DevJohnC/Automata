using System.Collections.Generic;
using System.Threading;

namespace Automata.HostServer.Infrastructure
{
    public interface IResourceProvider
    {
        IAsyncEnumerable<ResourceDocument> GetResources(CancellationToken cancellationToken = default);
    }
}