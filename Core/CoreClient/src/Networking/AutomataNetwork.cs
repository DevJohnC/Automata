using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client.Networking
{
    /// <summary>
    /// Network of Automata servers.
    /// </summary>
    public abstract class AutomataNetwork
    {
        private readonly List<IAutomataServer> _servers = new();

        public event EventHandler<IAutomataServer>? ServerAdded;
        
        public NetworkKindGraph KindGraph { get; } = new();

        public IReadOnlyCollection<IAutomataServer> Servers => _servers;

        public async Task AddServer(IAutomataServer server, CancellationToken cancellationToken = default)
        {
            await server.RefreshKindGraph(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            KindGraph.MergeServerGraph(server, server.KindGraph);
            _servers.Add(server);

            ServerAdded?.Invoke(this, server);
        }
    }
}