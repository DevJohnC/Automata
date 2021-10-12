using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client.Networking;
using Automata.Client.Resources;
using Automata.Kinds;

namespace Automata.Client
{
    public sealed class ServerResourceCollectionMonitor : IResourceCollectionMonitor
    {
        private readonly Dictionary<Guid, SerializedResourceGraph> _resourcesById = new();
        private readonly ObservableCollection<SerializedResourceGraph> _resources = new();

        private readonly CancellationTokenSource _tokenSource = new();
        private readonly KindName _resourceKind;
        private readonly IReadOnlyList<KindName> _associatedKinds;

        public IAutomataServer Server { get; }

        public ServerResourceCollectionMonitor(
            IAutomataServer server,
            KindName resourceKind,
            params KindName[] associatedKinds) :
            this(server, resourceKind, (IReadOnlyList<KindName>)associatedKinds)
        {
        }

        public ServerResourceCollectionMonitor(
            IAutomataServer server,
            KindName resourceKind,
            IReadOnlyList<KindName> associatedKinds)
        {
            _resourceKind = resourceKind;
            _associatedKinds = associatedKinds;
            Resources = new(_resources);
            Server = server;
            Run(_tokenSource.Token);
        }

        private async Task Run(CancellationToken cancellationToken)
        {
            try
            {
                await PopulateExistingResources(cancellationToken);
            }
            catch (TaskCanceledException e)
            {
            }
        }

        private async Task PopulateExistingResources(CancellationToken cancellationToken)
        {
            await using var resourcesClient = Server.CreateService<IResourceClient>();
            await foreach (var resourceGraph in resourcesClient.GetResources(
                _resourceKind, _associatedKinds)
                .WithCancellation(cancellationToken))
            {
                AddResource(resourceGraph);
            }
        }

        private void AddResource(SerializedResourceGraph resourceGraph)
        {
            var resourceId = resourceGraph.Resource.ResourceId;
            if (_resourcesById.ContainsKey(resourceId))
                return;
    
            _resourcesById.Add(resourceId, resourceGraph);
            _resources.Add(resourceGraph);
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
        }

        public ReadOnlyObservableCollection<SerializedResourceGraph> Resources { get; }
    }
}