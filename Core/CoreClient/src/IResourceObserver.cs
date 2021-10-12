using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client
{
    public interface IResourceObserver<TResource>
        where TResource : Record
    {
        Task ResourceAvailable(ResourceDocument<TResource> resource, CancellationToken cancellationToken);
        
        Task ResourceUnavailable(ResourceDocument<TResource> resource, CancellationToken cancellationToken);
        
        Task ResourceUpdated(ResourceDocument<TResource> resource, CancellationToken cancellationToken);
    }
}