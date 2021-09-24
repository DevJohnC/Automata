using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client
{
    public delegate void ServerDiscoveryEvent(Uri endPoint);
    
    public interface INetworkWatcher : IAsyncDisposable
    {
        event ServerDiscoveryEvent ServerAvailable;
        event ServerDiscoveryEvent ServerUnavailable;

        Task Start(CancellationToken cancellationToken);

        Task Stop(CancellationToken cancellationToken);
    }
}