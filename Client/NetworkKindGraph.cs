using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Automata.Kinds;

namespace Automata.Client
{
    public class NetworkKindGraph
    {
        private readonly Dictionary<GrpcAutomataServer, KindGraph> _serverGraphs = new();

        internal void MergeServerGraph(GrpcAutomataServer server, KindGraph serverKindGraph)
        {
            _serverGraphs[server] = serverKindGraph;
        }

        public bool TryGetKind(KindName kindName, [NotNullWhen(true)] out KindModel? kind)
        {
            foreach (var serverGraph in _serverGraphs.Values)
            {
                if (serverGraph.TryGetKind(kindName, out kind))
                    return true;
            }

            kind = default;
            return false;
        }
    }
}