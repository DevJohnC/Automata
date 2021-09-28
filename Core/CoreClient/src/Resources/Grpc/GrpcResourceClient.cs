using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.GrpcServices;
using Automata.Kinds;
using Grpc.Core;

namespace Automata.Client.Resources.Grpc
{
    public class GrpcResourceClient : IResourceClient
    {
        private readonly GrpcAutomataServer _server;
        private readonly ChannelBase _channel;

        public IAutomataServer Server => _server;

        public GrpcResourceClient(GrpcAutomataServer server, ChannelBase channel)
        {
            _server = server;
            _channel = channel;
        }
        
        public ValueTask DisposeAsync()
        {
            return default;
        }

        public IAsyncEnumerable<SerializedResourceGraph> GetResources(
            KindName resourceKind,
            IReadOnlyCollection<KindName>? associatedKinds = null)
        {
            return Impl();
            
            async IAsyncEnumerable<SerializedResourceGraph> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default
                )
            {
                var request = new ResourcesRequest
                {
                    Kind = new()
                    {
                        PluralUri = GrpcServices.PluralKindUri
                            .FromKindName(resourceKind)
                    }
                };
                if (associatedKinds?.Count > 0)
                {
                    foreach (var kindName in associatedKinds)
                    {
                        request.AssociatedKinds.Add(new GrpcServices.KindUri
                        {
                            PluralUri = GrpcServices.PluralKindUri.FromKindName(kindName) 
                        });
                    }
                }
                
                var client = new ResourcesService.ResourcesServiceClient(_channel);
                using var resourceStream = client.GetResources(request,
                    cancellationToken: cancellationToken);

                while (await resourceStream.ResponseStream.MoveNext())
                {
                    var currentRecord = resourceStream.ResponseStream.Current;
                    yield return new SerializedResourceGraph(
                        currentRecord.Resource.ToNative(),
                        currentRecord.AssociatedRecords.Select(
                            q => q.ToNative()).ToList());
                }
            }
        }
    }
}