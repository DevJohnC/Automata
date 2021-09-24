using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Client
{
    /// <summary>
    /// Network of Automata servers.
    /// </summary>
    public class AutomataNetwork : IAsyncDisposable
    {
        public delegate IAutomataServer ServerFactory(Uri endpoint);

        private readonly List<INetworkWatcher> _networkWatchers = new();
        private readonly List<WatcherFactoryPair> _watchersWithFactories = new();
        
        private readonly List<IAutomataServer> _servers = new();

        public event AsyncEventHandler<AutomataNetwork, IAutomataServer> ServerAdded;
        
        public NetworkKindGraph KindGraph { get; } = new();

        public IReadOnlyCollection<IAutomataServer> Servers => _servers;

        public IReadOnlyCollection<INetworkWatcher> NetworkWatchers => _networkWatchers;

        public async Task AddServer(IAutomataServer server, CancellationToken cancellationToken = default)
        {
            await server.RefreshKindGraph(cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            KindGraph.MergeServerGraph(server, server.KindGraph);
            _servers.Add(server);

            await ServerAdded?.SerialInvoke(this, server, cancellationToken);
        }

        public async Task AddNetworkWatcher(INetworkWatcher networkWatcher,
            ServerFactory serverFactory,
            CancellationToken cancellationToken = default)
        {
            var watcherPair = new WatcherFactoryPair(networkWatcher, serverFactory);
            watcherPair.ServerAvailable += NetworkWatcherOnServerAvailable;
            watcherPair.ServerUnavailable += NetworkWatcherOnServerUnavailable;
            await networkWatcher.Start(cancellationToken);
            _networkWatchers.Add(networkWatcher);
            _watchersWithFactories.Add(watcherPair);
        }
        
        private async void NetworkWatcherOnServerAvailable(INetworkWatcher watcher, ServerFactory factory, Uri endpoint)
        {
            try
            {
                await AddServer(factory(endpoint));
            }
            catch (Exception e)
            {
                //  todo: log
            }
        }

        private void NetworkWatcherOnServerUnavailable(INetworkWatcher watcher, ServerFactory factory, Uri endpoint)
        {
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var watcher in _networkWatchers)
            {
                try
                {
                    await watcher.DisposeAsync();
                }
                catch (Exception e)
                {
                    //  todo: log
                }
            }
        }

        private class WatcherFactoryPair
        {
            public delegate void DiscoveryEvent(INetworkWatcher watcher, ServerFactory factory, Uri endpoint);
            
            private readonly INetworkWatcher _watcher;
            private readonly ServerFactory _factory;
            public event DiscoveryEvent ServerAvailable;
            public event DiscoveryEvent ServerUnavailable;

            public WatcherFactoryPair(INetworkWatcher watcher, ServerFactory factory)
            {
                _watcher = watcher;
                _factory = factory;
                
                _watcher.ServerAvailable += WatcherOnServerAvailable;
                _watcher.ServerUnavailable += WatcherOnServerUnavailable;
            }

            private void WatcherOnServerUnavailable(Uri endpoint)
            {
                ServerUnavailable?.Invoke(_watcher, _factory, endpoint);
            }

            private void WatcherOnServerAvailable(Uri endpoint)
            {
                ServerAvailable?.Invoke(_watcher, _factory, endpoint);
            }
        }
    }
}