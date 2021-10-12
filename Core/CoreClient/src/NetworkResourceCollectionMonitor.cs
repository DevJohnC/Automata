using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Automata.Client.Networking;
using Automata.Client.Resources;
using Automata.Kinds;

namespace Automata.Client
{
    public class NetworkResourceCollectionMonitor : IResourceCollectionMonitor
    {
        private readonly KindName _resourceKind;
        private readonly IReadOnlyList<KindName> _associatedKinds;

        private readonly Dictionary<Guid, SerializedResourceGraph> _resourcesById = new();
        private readonly ObservableCollection<SerializedResourceGraph> _resources = new();
        
        private readonly List<ServerResourceCollectionMonitor> _serverMonitors = new();

        public NetworkResourceCollectionMonitor(
            AutomataNetwork network,
            KindName resourceKind,
            params KindName[] associatedKinds) :
            this(network, resourceKind, (IReadOnlyList<KindName>)associatedKinds)
        {
        }
        
        public NetworkResourceCollectionMonitor(
            AutomataNetwork network,
            KindName resourceKind,
            IReadOnlyList<KindName> associatedKinds)
        {
            Network = network;
            _resourceKind = resourceKind;
            _associatedKinds = associatedKinds;
            Resources = new(_resources);

            //  todo: hook into when servers become available and unavailable on the network
            foreach (var server in network.Servers)
            {
                AddServer(server);
            }

            network.ServerAdded += (_, server) => AddServer(server);
        }

        private void AddServer(IAutomataServer server)
        {
            var monitor = new ServerResourceCollectionMonitor(server, _resourceKind, _associatedKinds);
            HookCollectionUpdates(monitor.Resources);
            _serverMonitors.Add(monitor);
            //  it's possible the monitor already collected (some or all) resources from the server before we hooked into update events
            //  so add all the resources discovered so far
            AddServerResources(monitor);
        }

        private void HookCollectionUpdates(INotifyCollectionChanged collection)
        {
            collection.CollectionChanged += CollectionUpdated;
        }

        private void AddServerResources(ServerResourceCollectionMonitor monitor)
        {
            //  todo: is it possible we'd get a collection modified exception? how to guard against that?
            foreach (var resource in monitor.Resources)
            {
                AddResource(resource);
            }
        }

        private void CollectionUpdated(object? sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SerializedResourceGraph resource in e.NewItems)
                        AddResource(resource);
                    break;
            }
        }
        
        private void AddResource(SerializedResourceGraph resourceGraph)
        {
            lock (_resourcesById)
            {
                var resourceId = resourceGraph.Resource.ResourceId;
                if (_resourcesById.ContainsKey(resourceId))
                    return;

                _resourcesById.Add(resourceId, resourceGraph);
                _resources.Add(resourceGraph);
            }
        }

        public AutomataNetwork Network { get; }

        public void Dispose()
        {
            foreach (var monitor in _serverMonitors)
            {
                monitor.Dispose();
            }
        }

        public ReadOnlyObservableCollection<SerializedResourceGraph> Resources { get; }
    }
}