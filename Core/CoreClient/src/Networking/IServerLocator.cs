using System;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client.Networking
{
    public interface IServerLocator : IDisposable
    {
        event AsyncEventHandler<IServerLocator, Uri>? ServerAvailable;
        event AsyncEventHandler<IServerLocator, Uri>? ServerUnavailable;

        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}