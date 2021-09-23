namespace Automata.Client.Services
{
    public static class ServerServiceProviderExtensions
    {
        public static void TryRegister<TServer, TService>(
            this IServerServiceProvider<TServer> serviceProvider,
            ServerServiceFactory<TServer, TService> factory)
            where TServer : IAutomataServer
            where TService : IAutomataNetworkService
        {
            if (serviceProvider.IsServiceRegistered<TService>())
                return;
            serviceProvider.Register(factory);
        }
    }
}