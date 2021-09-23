namespace Automata.Client.Services
{
    /// <summary>
    /// Provides client networking services to an automata server implementation.
    /// </summary>
    /// <remarks>
    /// This is meant to serve as a means to extend automata behavior with new modules but not
    /// act as a general services container ala. aspnetcore.
    ///
    /// Service locators are generally an anti-pattern so please don't use this type
    /// to start registering all manner of services for use within your application.
    ///
    /// This is why the <see cref="IAutomataNetworkService"/> marker interface is used, to help
    /// enforce the desired pattern.
    /// </remarks>
    /// <typeparam name="TServer"></typeparam>
    public interface IServerServiceProvider<TServer>
        where TServer : IAutomataServer
    {
        void Register<TService>(ServerServiceFactory<TServer, TService> factory)
            where TService : IAutomataNetworkService;
        
        bool IsServiceRegistered<TService>()
            where TService : IAutomataNetworkService;

        TService GetService<TService>(TServer server)
            where TService : IAutomataNetworkService;
    }
}