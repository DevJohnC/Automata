using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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

        public IAsyncEnumerable<SerializedResourceDocument> GetResources(KindName resourceKind)
        {
            return Impl();
            
            async IAsyncEnumerable<SerializedResourceDocument> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default
                )
            {
                var client = new GrpcServices.ResourcesService.ResourcesServiceClient(_channel);
                using var resourceStream = client.GetResources(new()
                {
                    PluralUri = GrpcServices.PluralKindUri
                        .FromKindName(resourceKind)
                }, cancellationToken: cancellationToken);

                while (await resourceStream.ResponseStream.MoveNext())
                {
                    var currentRecord = resourceStream.ResponseStream.Current;
                    yield return currentRecord.ToNative();
                }
            }
        }
    }
}