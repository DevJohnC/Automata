using System;
using System.Collections.Generic;
using System.Threading;
using Automata.Client.Services;
using Automata.Kinds;

namespace Automata.Client.Resources
{
    public record SerializedResourceGraph(
        SerializedResourceDocument Resource,
        IReadOnlyList<SerializedResourceDocument> AssociatedResources);
    
    public interface IResourceClient : IAutomataNetworkService, IAsyncDisposable
    {
        IAsyncEnumerable<SerializedResourceGraph> GetResources(
            KindName resourceKind,
            IReadOnlyCollection<KindName>? associatedKinds = null);
    }
}