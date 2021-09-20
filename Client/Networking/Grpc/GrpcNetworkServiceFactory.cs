using System;
using System.Net.Http;
using Grpc.Core;
using Grpc.Net.Client;

namespace Automata.Client.Networking.Grpc
{
    public class GrpcNetworkServiceFactory : INetworkServiceFactory<INetworkClient>
    {
        private readonly IGrpcChannelFactory _channelFactory;

        public GrpcNetworkServiceFactory(IGrpcChannelFactory? channelFactory = null)
        {
            _channelFactory = channelFactory ??
                              InsecureChannelFactory.SharedInstance;
        }
        
        public INetworkClient CreateClient(GrpcAutomataServer server)
        {
            return new GrpcNetworkClient(_channelFactory.CreateChannel(server));
        }
    }
}