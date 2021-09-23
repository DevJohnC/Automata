using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Automata.Client;
using Automata.Devices.GrpcServices;
using Automata.Kinds;
using Grpc.Core;

namespace Automata.Devices
{
    public class GrpcDevicesClient : IDevicesClient
    {
        public static IDevicesClient Factory(GrpcAutomataServer server)
        {
            return new GrpcDevicesClient(
                server,
                server.ChannelFactory.CreateChannel(server));
        }
        
        private readonly DeviceServices.DeviceServicesClient _client;
        
        private readonly GrpcAutomataServer _server;
        
        public IAutomataServer Server => _server;
        
        public GrpcDevicesClient(GrpcAutomataServer server, ChannelBase channel)
        {
            _server = server;
            _client = new DeviceServices.DeviceServicesClient(channel);
        }

        public IAsyncEnumerable<DeviceStatePair> GetDeviceStatePairs(KindUri deviceKindUri, KindUri stateKindUri)
        {
            return Impl();
            
            async IAsyncEnumerable<DeviceStatePair> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                using var response = _client.GetDeviceStatePairs(new()
                {
                    DeviceKind = Automata.GrpcServices.KindUri.FromNative(deviceKindUri),
                    StateKind = Automata.GrpcServices.KindUri.FromNative(stateKindUri)
                });
                var stream = response.ResponseStream;
                while (await stream.MoveNext(cancellationToken))
                {
                    yield return new DeviceStatePair(
                        stream.Current.DeviceDefinition.ToNative(),
                        stream.Current.DeviceState.ToNative()
                    );
                }
            }
        }
    }
}