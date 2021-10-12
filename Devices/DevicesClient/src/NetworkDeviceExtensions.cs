using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using async_enumerable_dotnet;
using Automata.Client;
using Automata.Client.Networking;
using Automata.Client.Resources;
using Automata.Kinds;

namespace Automata.Devices
{
    public static class NetworkDeviceExtensions
    {
        private static readonly KindModel StateKind = KindModel.GetKind(typeof(DeviceState));
        
        public static IAsyncEnumerable<Device<TDevice>> GetDevices<TDevice>(
            this AutomataNetwork network)
            where TDevice : notnull, DeviceDefinition
        {
            return Impl();
            
            async IAsyncEnumerable<Device<TDevice>> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                List<IAsyncEnumerable<Device<TDevice>>> streams = new();
                foreach (var server in network.Servers)
                {
                    streams.Add(server.GetDevices<TDevice>());
                }

                await foreach (var deviceHandle in AsyncEnumerable
                    .Merge(streams.ToArray())
                    .WithCancellation(cancellationToken))
                {
                    yield return deviceHandle;
                }
            }
        }

        public static IAsyncEnumerable<Device<TDevice>> GetDevices<TDevice>(
            this IAutomataServer server)
            where TDevice : notnull, DeviceDefinition
        {
            return server.CreateService<IResourceClient>().GetDevices<TDevice>();
        }
        
        public static IAsyncEnumerable<Device<TDevice>> GetDevices<TDevice>(
            this IResourceClient resourceClient)
            where TDevice : notnull, DeviceDefinition
        {
            var deviceKind = KindModel.GetKind(typeof(TDevice));

            return Impl();
            
            async IAsyncEnumerable<Device<TDevice>> Impl(
                [EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                await foreach (var graph in resourceClient
                    .GetResources(deviceKind.Name, new[]
                    {
                        StateKind.Name
                    })
                    .WithCancellation(cancellationToken))
                {
                    var deviceRecord = graph.Resource.Deserialize<TDevice>();
                    yield return new Device<TDevice>(
                        deviceRecord.ResourceId,
                        deviceRecord.Record,
                        graph.AssociatedResources);
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