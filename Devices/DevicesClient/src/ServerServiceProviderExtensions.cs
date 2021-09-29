using Automata.Client;
using Automata.Client.Services;
using Automata.Events;

namespace Automata.Devices
{
    public static class ServerServiceProviderExtensions
    {
        public static IServerServiceProvider<GrpcAutomataServer> AddDevices(
            this IServerServiceProvider<GrpcAutomataServer> serviceProvider)
        {
            serviceProvider.AddEvents();
            serviceProvider.TryRegister(GrpcStateClient.Factory);
            return serviceProvider;
        }
    }
}