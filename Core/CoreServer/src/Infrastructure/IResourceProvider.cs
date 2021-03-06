using System.Collections.Generic;
using System.Threading;

namespace Automata.HostServer.Infrastructure
{
    public interface IResourceProvider
    {
        IAsyncEnumerable<ResourceDocument> GetResources();

        IAsyncEnumerable<ResourceDocument> GetAssociatedResources(ResourceIdentifier resourceIdentifier);
    }
}