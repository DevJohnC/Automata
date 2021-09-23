using System;
using System.Collections.Generic;
using System.Threading;
using Automata.Kinds;

namespace Automata.Client.Resources
{
    public interface IResourceClient : IAutomataNetworkService, IAsyncDisposable
    {
        IAsyncEnumerable<SerializedResourceDocument> GetResources(KindName resourceKind);
    }
}