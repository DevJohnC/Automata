using Automata.Client;
using Automata.Client.Networking;
using Automata.Devices.Networking;
using Automata.Devices.Networking.Grpc;

namespace Automata.Devices
{
    public class GrpcDevicesClient : DevicesClient
    {
        public GrpcDevicesClient(GrpcAutomataNetwork network) :
            base(network, new ServiceFactory())
        {
        }

        private class ServiceFactory : INetworkServiceFactory<IDevicesService>
        {
            public IDevicesService CreateClient(GrpcAutomataServer server)
            {
                return new GrpcDevicesService(
                    server.ChannelFactory.CreateChannel(server));
            }
        }
    }
}