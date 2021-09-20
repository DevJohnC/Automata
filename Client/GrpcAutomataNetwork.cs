using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client
{
    /// <summary>
    /// Network of Automata servers that communicate using gRPC services.
    /// </summary>
    public class GrpcAutomataNetwork
    {
        private readonly List<GrpcAutomataServer> _servers = new();
        
        public NetworkKindGraph KindGraph { get; } = new();

        public IReadOnlyCollection<GrpcAutomataServer> Servers => _servers;
        
        public async Task AddServer(GrpcAutomataServer server, CancellationToken cancellationToken)
        {
            await server.UpdateKindGraph(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            KindGraph.MergeServerGraph(server, server.KindGraph);
            _servers.Add(server);
        }
    }
}