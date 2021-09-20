using System.Runtime.CompilerServices;
using Grpc.Core;

namespace Automata.Client.Networking.Grpc
{
    public class SharedChannelFactory : IGrpcChannelFactory
    {
        private readonly IGrpcChannelFactory _channelFactory;
        private readonly ConditionalWeakTable<GrpcAutomataServer, ChannelBase> _channels = new();

        public SharedChannelFactory(IGrpcChannelFactory channelFactory)
        {
            _channelFactory = channelFactory;
        }
        
        public ChannelBase CreateChannel(GrpcAutomataServer server)
        {
            lock (_channels)
            {
                if (!_channels.TryGetValue(server, out var channel))
                {
                    channel = _channelFactory.CreateChannel(server);
                    _channels.Add(server, channel);
                }

                return channel;
            }
        }
    }
}