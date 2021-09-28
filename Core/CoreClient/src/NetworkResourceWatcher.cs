using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Automata.Client
{
    //  todo: need server added and server removed events
    //  todo: when a server goes off network any hosted resources should get unavailable events raised
    //  todo: filter resource kinds to watch
    //  todo: maybe an observer pattern to watch resources with a generic type parameter
    public class NetworkResourceWatcher : BackgroundService, IResourceWatcher, IAsyncDisposable
    {
        private readonly IServerLocator[] _serverDiscoverers;

        public record ServerEventArgs(Uri ServerUri);

        private readonly List<ServerWatcher> _serverWatchers = new();

        public event AsyncEventHandler<NetworkResourceWatcher, ServerEventArgs>? ServerAvailable;

        public event AsyncEventHandler<NetworkResourceWatcher, ServerEventArgs>? ServerUnavailable;
        
        public event AsyncEventHandler<NetworkResourceWatcher, ServerResourceEventArgs>? ResourceAvailable;
        
        public event AsyncEventHandler<NetworkResourceWatcher, ServerResourceEventArgs>? ResourceChanged;
        
        public event AsyncEventHandler<NetworkResourceWatcher, ServerResourceEventArgs>? ResourceUnavailable;

        public AutomataNetwork Network { get; }

        public NetworkResourceWatcher(AutomataNetwork network,
            params IServerLocator[] serverDiscoverers)
        {
            _serverDiscoverers = serverDiscoverers;
            Network = network;
        }

        private void HookServerDiscoveryEvents(IServerLocator serverLocator)
        {
            serverLocator.ServerAvailable += ServerDiscovererOnServerAvailable;
            serverLocator.ServerUnavailable += ServerDiscovererOnServerUnavailable;
        }

        private void UnhookServerDiscoveryEvents(IServerLocator serverLocator)
        {
            serverLocator.ServerAvailable -= ServerDiscovererOnServerAvailable;
            serverLocator.ServerUnavailable -= ServerDiscovererOnServerUnavailable;
        }
        
        private Task ServerDiscovererOnServerAvailable(IServerLocator sender, Uri endpoint, CancellationToken ct)
        {
            return ServerAvailable?.SerialInvoke(this, new(endpoint), ct) ??
                   Task.CompletedTask;
        }
        
        private Task ServerDiscovererOnServerUnavailable(IServerLocator sender, Uri endpoint, CancellationToken ct)
        {
            return ServerUnavailable?.SerialInvoke(this, new(endpoint), ct) ??
                   Task.CompletedTask;
        }

        private void HookNetworkEvents(AutomataNetwork network)
        {
            network.ServerAdded += NetworkOnServerAdded;
        }

        private void UnhookNetworkEvents(AutomataNetwork network)
        {
            network.ServerAdded -= NetworkOnServerAdded;
        }

        private Task NetworkOnServerAdded(AutomataNetwork sender, IAutomataServer e, CancellationToken cancellationToken)
        {
            var watcher = new ServerWatcher(e);
            HookServerEvents(watcher);
            _serverWatchers.Add(watcher);
            return watcher.StartAsync(cancellationToken);
        }

        private void HookServerEvents(ServerWatcher serverWatcher)
        {
            serverWatcher.ResourceAvailable += ServerWatcherOnResourceAvailable;
            serverWatcher.ResourceUnavailable += ServerWatcherOnResourceUnavailable;
            serverWatcher.ResourceChanged += ServerWatcherOnResourceChanged;
        }

        private void UnhookServerEvents(ServerWatcher serverWatcher)
        {
            serverWatcher.ResourceAvailable -= ServerWatcherOnResourceAvailable;
            serverWatcher.ResourceUnavailable -= ServerWatcherOnResourceUnavailable;
            serverWatcher.ResourceChanged -= ServerWatcherOnResourceChanged;
        }
        
        private Task ServerWatcherOnResourceAvailable(ServerWatcher sender, ServerResourceEventArgs e, CancellationToken cancellationtoken)
        {
            return ResourceAvailable?.SerialInvoke(this, e, cancellationtoken) ??
                   Task.CompletedTask;
        }
        
        private Task ServerWatcherOnResourceUnavailable(ServerWatcher sender, ServerResourceEventArgs e, CancellationToken cancellationtoken)
        {
            return ResourceUnavailable?.SerialInvoke(this, e, cancellationtoken) ??
                   Task.CompletedTask;
        }
        
        private Task ServerWatcherOnResourceChanged(ServerWatcher sender, ServerResourceEventArgs e, CancellationToken cancellationtoken)
        {
            return ResourceChanged?.SerialInvoke(this, e, cancellationtoken) ??
                   Task.CompletedTask;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                foreach (var discoverer in _serverDiscoverers)
                {
                    HookServerDiscoveryEvents(discoverer);
                    await discoverer.StartAsync(stoppingToken);
                }
                
                foreach (var server in Network.Servers)
                {
                    var watcher = new ServerWatcher(server);
                    _serverWatchers.Add(watcher);
                    HookServerEvents(watcher);
                    await watcher.StartAsync(stoppingToken);
                }

                HookNetworkEvents(Network);
                
                while (true)
                {
                    await Task.Delay(-1, stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
            finally
            {
                UnhookNetworkEvents(Network);
                foreach (var discoverer in _serverDiscoverers)
                {
                    await discoverer.StopAsync(stoppingToken);
                    UnhookServerDiscoveryEvents(discoverer);
                }

                foreach (var watcher in _serverWatchers)
                {
                    await watcher.StopAsync(stoppingToken);
                    UnhookServerEvents(watcher);
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await StopAsync(cts.Token);
        }
    }
}