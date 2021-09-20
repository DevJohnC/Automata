/*using Automata.Client;
using Automata.Client.Networking;
using Automata.Client.Networking.Grpc;

namespace Automata.Devices.Networking.Grpc
{
    public class GrpcDeviceServiceFactory : INetworkServiceFactory<IDevicesService>
    {
        private readonly IGrpcChannelFactory _channelFactory;

        public GrpcDeviceServiceFactory(IGrpcChannelFactory? channelFactory = null)
        {
            _channelFactory = channelFactory ??
                              InsecureChannelFactory.SharedInstance;
        }
        
        public IDevicesService CreateClient(GrpcAutomataServer server)
        {
            return new GrpcDevicesService(_channelFactory.CreateChannel(server));
        }
    }
}*/