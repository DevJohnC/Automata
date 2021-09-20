using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.Devices.GrpcServices;
using Automata.Kinds;
using Grpc.Core;

namespace Automata.Devices.Networking.Grpc
{
    public class GrpcDevicesService : IDevicesService
    {
        private readonly ChannelBase _channel;

        public GrpcDevicesService(ChannelBase channel)
        {
            _channel = channel;
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public async IAsyncEnumerable<DeviceStatePair> GetDevicesFromServer(
            KindName deviceKind, KindName stateKind,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var client = new DeviceServices.DeviceServicesClient(_channel);
            using var response = client.GetDeviceStatePairs(new()
            {
                DeviceKind = new()
                {
                    PluralUri = Automata.GrpcServices.PluralKindUri
                        .FromKindName(deviceKind) 
                },
                StateKind = new()
                {
                    PluralUri = Automata.GrpcServices.PluralKindUri
                        .FromKindName(stateKind)
                }
            });
            var stream = response.ResponseStream;
            while (await stream.MoveNext(cancellationToken))
            {
                yield return new DeviceStatePair(
                    stream.Current.DeviceDefinition.ToResourceDocument(),
                    stream.Current.DeviceState.ToResourceDocument()
                );
            }
        }
    }
}