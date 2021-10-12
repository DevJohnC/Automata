using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client.Networking
{
    public abstract class AdHocNetwork : AutomataNetwork, IDisposable
    {
        private readonly IServerLocator[] _serverLocators;

        public AdHocNetwork(params IServerLocator[] serverLocators)
        {
            _serverLocators = serverLocators;
            foreach (var serverLocator in _serverLocators)
            {
                HookServerLocator(serverLocator);
                serverLocator.StartAsync(default);
            }
        }

        public AdHocNetwork(IEnumerable<IServerLocator> serverLocators) :
            this(serverLocators.ToArray())
        {
        }

        private void HookServerLocator(IServerLocator serverLocator)
        {
            serverLocator.ServerAvailable += ServerDiscovered;
        }

        private async Task ServerDiscovered(IServerLocator sender, Uri uri, CancellationToken ct)
        {
            var server = await CreateServer(sender, uri, ct);
            if (server == null)
                return;
            await AddServer(server, ct);
        }

        protected abstract Task<IAutomataServer?> CreateServer(IServerLocator locator, Uri uri, CancellationToken ct);

        public void Dispose()
        {
            foreach (var serverLocator in _serverLocators)
            {
                serverLocator.Dispose();
            }
        }
    }
}