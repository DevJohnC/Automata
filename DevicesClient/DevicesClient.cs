using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using async_enumerable_dotnet;
using Automata.Client;
using Automata.Client.Networking;
using Automata.Devices.Networking;
using Automata.Kinds;

namespace Automata.Devices
{
    public abstract class DevicesClient
    {
        private readonly INetworkServiceFactory<IDevicesService> _deviceServiceFactory;

        protected DevicesClient(GrpcAutomataNetwork network,
                            INetworkServiceFactory<IDevicesService> deviceServiceFactory)
        {
            _deviceServiceFactory = deviceServiceFactory;
            Network = network;
        }

        public GrpcAutomataNetwork Network { get; }

        public IAsyncEnumerable<DeviceHandle<DeviceDefinition, DeviceState>> GetDevices(
            CancellationToken cancellationToken = default)
        {
            return GetDevices<DeviceDefinition, DeviceState>(cancellationToken);
        }

        public async IAsyncEnumerable<DeviceHandle<TDevice, TState>> GetDevices<TDevice, TState>(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TDevice : notnull, DeviceDefinition
            where TState : notnull, DeviceState
        {
            var deviceKind = KindModel.GetKind(typeof(TDevice));
            var stateKind = KindModel.GetKind(typeof(TState));
            
            List<IAsyncEnumerable<DeviceHandle<TDevice, TState>>> streams = new();
            foreach (var server in Network.Servers)
            {
                streams.Add(GetDevicesFromServer<TDevice, TState>(
                    deviceKind, stateKind,
                    server, cancellationToken));
            }

            await foreach (var deviceHandle in AsyncEnumerable
                .Merge(streams.ToArray())
                .WithCancellation(cancellationToken))
            {
                yield return deviceHandle;
            }
        }

        private async IAsyncEnumerable<DeviceHandle<TDevice, TState>> GetDevicesFromServer<TDevice, TState>(
            KindModel deviceKind,
            KindModel stateKind,
            GrpcAutomataServer server,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TDevice : notnull, DeviceDefinition
            where TState : notnull, DeviceState
        {
            await using var client = _deviceServiceFactory.CreateClient(server);
            await foreach (var pair in client
                .GetDevicesFromServer(deviceKind.Name, stateKind.Name, cancellationToken))
            {
                yield return CreateDeviceHandle<TDevice, TState>(Network, server, pair);
            }
        }

        private static DeviceHandle<TDevice, TState> CreateDeviceHandle<TDevice, TState>(
            GrpcAutomataNetwork network,
            GrpcAutomataServer server,
            DeviceStatePair deviceStatePair)
            where TDevice : notnull, DeviceDefinition
            where TState : notnull, DeviceState
        {
            return new DeviceHandle<TDevice, TState>(
                network,
                server,
                deviceStatePair.DeviceDefinition
                    .Deserialize<TDevice>(),
                deviceStatePair.DeviceState
                    .Deserialize<TState>());
        }
    }
}