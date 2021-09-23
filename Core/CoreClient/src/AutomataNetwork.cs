using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client
{
    /// <summary>
    /// Network of Automata servers.
    /// </summary>
    public class AutomataNetwork
    {
        private readonly List<IAutomataServer> _servers = new();
        
        public NetworkKindGraph KindGraph { get; } = new();

        public IReadOnlyCollection<IAutomataServer> Servers => _servers;
        
        public async Task AddServer(IAutomataServer server, CancellationToken cancellationToken)
        {
            await server.RefreshKindGraph(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            KindGraph.MergeServerGraph(server, server.KindGraph);
            _servers.Add(server);
        }
    }
}