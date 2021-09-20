using System;

namespace Automata.Client.Networking
{
    public interface INetworkServiceFactory<TNetworkService>
    {
        TNetworkService CreateClient(GrpcAutomataServer server);
    }
}