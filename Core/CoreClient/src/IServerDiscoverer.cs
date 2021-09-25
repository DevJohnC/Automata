using System;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client
{
    public interface IServerDiscoverer : IAsyncDisposable
    {
        event AsyncEventHandler<IServerDiscoverer, Uri>? ServerAvailable;
        event AsyncEventHandler<IServerDiscoverer, Uri>? ServerUnavailable;

        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}