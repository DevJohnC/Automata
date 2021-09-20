using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client.Networking;
using Automata.Client.Networking.Grpc;
using Automata.Kinds;

namespace Automata.Client
{
    public class GrpcAutomataServer
    {
        private readonly INetworkServiceFactory<INetworkClient> _serviceFactory;
        
        public KindGraph? KindGraph { get; private set; }
        
        public Uri ServerUri { get; }
        
        public IGrpcChannelFactory ChannelFactory { get; }

        public GrpcAutomataServer(string serverUri, IGrpcChannelFactory? grpcChannelFactory = null) :
            this(new Uri(serverUri), grpcChannelFactory)
        {
        }

        public GrpcAutomataServer(Uri serverUri, IGrpcChannelFactory? grpcChannelFactory = null)
        {
            ServerUri = serverUri;
            ChannelFactory = grpcChannelFactory ?? InsecureChannelFactory.SharedInstance;
            _serviceFactory = new GrpcNetworkServiceFactory(ChannelFactory);
        }

        public IAsyncEnumerable<SerializedResourceDocument> GetResources(KindName? kindName = null)
        {
            return Impl(default);
            
            async IAsyncEnumerable<SerializedResourceDocument> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                await using var client = _serviceFactory.CreateClient(this);
                await foreach (var resource in client.GetResources(kindName ?? KindModel.GetKind(typeof(Record)).Name,
                    cancellationToken))
                {
                    yield return resource;
                }
            }
        }
        
        public IAsyncEnumerable<ResourceDocument<T>> GetResources<T>()
            where T : Record
        {
            return Impl(default);
            
            async IAsyncEnumerable<ResourceDocument<T>> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                await using var client = _serviceFactory.CreateClient(this);
                await foreach (var resource in client.GetResources<T>(cancellationToken))
                {
                    yield return resource;
                }
            }
        }

        [MemberNotNull(nameof(KindGraph))]
        public async Task UpdateKindGraph(CancellationToken cancellationToken)
        {
            var graphBuilder = new KindGraphBuilder(new());
            await using var client = _serviceFactory.CreateClient(this);
            await foreach (var kindResource in client.GetResources<KindRecord>(cancellationToken))
            {
                graphBuilder = graphBuilder.DefineKind(kindResource);
            }

            KindGraph = graphBuilder.Build();
        }
    }
}