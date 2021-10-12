using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client.Networking.Grpc;
using Automata.Client.Resources;
using Automata.Client.Resources.Grpc;
using Automata.Client.Services;
using Automata.Kinds;

namespace Automata.Client.Networking
{
    public class GrpcAutomataServer : IAutomataServer
    {
        private readonly IServerServiceProvider<GrpcAutomataServer> _services;

        public KindGraph? KindGraph { get; private set; }
        
        public Uri ServerUri { get; }
        
        public IGrpcChannelFactory ChannelFactory { get; }

        public GrpcAutomataServer(
            string serverUri,
            IServerServiceProvider<GrpcAutomataServer> services,
            IGrpcChannelFactory? grpcChannelFactory = null) :
            this(new Uri(serverUri), services, grpcChannelFactory)
        {
        }

        public GrpcAutomataServer(
            Uri serverUri,
            IServerServiceProvider<GrpcAutomataServer> services,
            IGrpcChannelFactory? grpcChannelFactory = null)
        {
            ServerUri = serverUri;
            ChannelFactory = grpcChannelFactory ?? InsecureChannelFactory.SharedInstance;
            _services = services;
            _services.TryRegister<GrpcAutomataServer, IResourceClient>(server => new GrpcResourceClient(
                server,
                ChannelFactory.CreateChannel(server)));
        }

        [MemberNotNull(nameof(KindGraph))]
        public async Task RefreshKindGraph(CancellationToken cancellationToken = default)
        {
            var graphBuilder = new KindGraphBuilder(new());
            await using var client = _services.GetService<IResourceClient>(this);
            await foreach (var kindResource in client.GetResources<KindRecord>()
                .WithCancellation(cancellationToken))
            {
                graphBuilder = graphBuilder.DefineKind(kindResource);
            }

            KindGraph = graphBuilder.Build();
        }

        public bool SupportsService<TService>()
            where TService : IAutomataNetworkService
        {
            return _services.IsServiceRegistered<TService>();
        }

        public TService CreateService<TService>()
            where TService : IAutomataNetworkService
        {
            return _services.GetService<TService>(this);
        }
    }
}