using Automata.Client;
using Automata.Client.Networking;
using Automata.Client.Services;

namespace Automata.Events
{
    public static class ServerServiceProviderExtensions
    {
        public static IServerServiceProvider<GrpcAutomataServer> AddEvents(
            this IServerServiceProvider<GrpcAutomataServer> serviceProvider)
        {
            serviceProvider.TryRegister(GrpcEventsClient.Factory);
            return serviceProvider;
        }
    }
}