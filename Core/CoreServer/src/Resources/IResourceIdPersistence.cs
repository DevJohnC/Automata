using System;
using System.Threading.Tasks;
using Automata.Kinds;

namespace Automata.HostServer.Resources
{
    public interface IResourceIdPersistence
    {
        Guid GetOrCreateResourceId(KindName kindName, string resourceKey);
        
        Task<Guid> GetOrCreateResourceIdAsync(KindName kindName, string resourceKey);
    }
}