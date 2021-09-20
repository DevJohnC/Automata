using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.Kinds;
using Grpc.Core;

namespace Automata.Client.Networking.Grpc
{
    public class GrpcNetworkClient : INetworkClient
    {
        private readonly ChannelBase _channel;

        public GrpcNetworkClient(ChannelBase channel)
        {
            _channel = channel;
        }
        
        public ValueTask DisposeAsync()
        {
            return default;
        }

        public async IAsyncEnumerable<SerializedResourceDocument> GetResources(
            KindName resourceKind,
            [EnumeratorCancellation] CancellationToken cancellationToken)
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
                yield return currentRecord.ToResourceDocument();
            }
        }
    }
}