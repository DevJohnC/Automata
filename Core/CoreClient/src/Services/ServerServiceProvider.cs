using System;
using System.Collections.Generic;

namespace Automata.Client.Services
{
    public delegate TService ServerServiceFactory<TServer, TService>(TServer server)
        where TServer : IAutomataServer
        where TService : IAutomataNetworkService;

    public class ServerServiceProvider<TServer> : IServerServiceProvider<TServer>
        where TServer : IAutomataServer
    {
        private readonly Dictionary<Type, Factory> _factories = new();
        
        public void Register<TService>(ServerServiceFactory<TServer, TService> factory)
            where TService : IAutomataNetworkService
        {
            _factories.Add(typeof(TService), new Factory<TService>(factory));
        }
        
        public bool IsServiceRegistered<TService>()
            where TService : IAutomataNetworkService
        {
            return _factories.ContainsKey(typeof(TService));
        }

        public TService GetService<TService>(TServer server)
            where TService : IAutomataNetworkService
        {
            if (!_factories.TryGetValue(typeof(TService), out var factory) ||
                factory is not Factory<TService> serviceFactory)
                throw new MissingFactoryException();

            return serviceFactory.GetService(server);
        }

        private abstract class Factory
        {
        }

        private class Factory<TService> : Factory
            where TService : IAutomataNetworkService
        {
            private readonly ServerServiceFactory<TServer, TService> _factory;

            public Factory(ServerServiceFactory<TServer, TService> factory)
            {
                _factory = factory;
            }

            public TService GetService(TServer server)
            {
                return _factory(server);
            }
        }
    }
}