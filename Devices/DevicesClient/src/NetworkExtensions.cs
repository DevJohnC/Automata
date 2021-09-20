using System.Threading;
using System.Threading.Tasks;
using Automata.Kinds;

namespace Automata.Devices
{
    public static class NetworkExtensions
    {
        public static GrpcDevicesClient CreateDevicesClient(
            this Client.GrpcAutomataNetwork network)
        {
            return new GrpcDevicesClient(network);
        }
        
        public static async Task<GrpcStateClient> CreateStateClient(
            this Client.GrpcAutomataNetwork network,
            CancellationToken cancellationToken = default)
        {
            var client = new GrpcStateClient(network);
            await client.RefreshStateControllers(cancellationToken);
            return client;
        }

        public static bool SupportsDevices(this Client.GrpcAutomataServer server)
        {
            var kind = KindModel.GetKind(typeof(DeviceDefinition));
            return server.KindGraph?.TryGetKind(kind.Name, out _)
                ?? false;
        }
    }
}