using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using async_enumerable_dotnet;
using Automata.Client;
using Automata.Kinds;

namespace Automata.Devices
{
    public static class NetworkDeviceExtensions
    {
        public static IAsyncEnumerable<DeviceHandle<TDevice, TState>> GetDevices<TDevice, TState>(
            this AutomataNetwork network)
            where TDevice : notnull, DeviceDefinition
            where TState : notnull, DeviceState
        {
            return Impl();
            
            async IAsyncEnumerable<DeviceHandle<TDevice, TState>> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                List<IAsyncEnumerable<DeviceHandle<TDevice, TState>>> streams = new();
                foreach (var server in network.Servers)
                {
                    streams.Add(server.GetDevices<TDevice, TState>());
                }

                await foreach (var deviceHandle in AsyncEnumerable
                    .Merge(streams.ToArray())
                    .WithCancellation(cancellationToken))
                {
                    yield return deviceHandle;
                }
            }
        }

        public static IAsyncEnumerable<DeviceHandle<TDevice, TState>> GetDevices<TDevice, TState>(
            this IAutomataServer server)
            where TDevice : notnull, DeviceDefinition
            where TState : notnull, DeviceState
        {
            return server.CreateService<IDevicesClient>().GetDevices<TDevice, TState>();
        }
        
        public static IAsyncEnumerable<DeviceHandle<TDevice, TState>> GetDevices<TDevice, TState>(
            this IDevicesClient devicesClient)
            where TDevice : notnull, DeviceDefinition
            where TState : notnull, DeviceState
        {
            var deviceKind = KindModel.GetKind(typeof(TDevice));
            var stateKind = KindModel.GetKind(typeof(TState));
            
            return Impl();
            
            async IAsyncEnumerable<DeviceHandle<TDevice, TState>> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                await foreach (var pair in devicesClient
                    .GetDeviceStatePairs(deviceKind.Name.PluralUri, stateKind.Name.PluralUri)
                    .WithCancellation(cancellationToken))
                {
                    yield return new DeviceHandle<TDevice, TState>(
                        devicesClient.Server,
                        pair.DeviceDefinition.Deserialize<TDevice>(),
                        pair.DeviceState.Deserialize<TState>());
                }
            }
        }

        public static bool SupportsDevices(this IAutomataServer server)
        {
            var kind = KindModel.GetKind(typeof(DeviceDefinition));
            return server.KindGraph?.TryGetKind(kind.Name, out _)
                ?? false;
        }
    }
}