using System;
using System.Collections.Generic;
using System.Threading;
using Automata.Kinds;

namespace Automata.Client.Networking
{
    public interface INetworkClient : IAsyncDisposable
    {
        IAsyncEnumerable<SerializedResourceDocument> GetResources(
            KindName resourceKind,
            CancellationToken cancellationToken);
    }
}