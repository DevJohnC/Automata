using System.Collections.Generic;
using System.Threading;
using Automata.Kinds;

namespace Automata.HostServer.Api
{
    public interface IResourceApi
    {
        IAsyncEnumerable<SerializedResourceDocument> GetResources(KindUri kindUri);
    }
}