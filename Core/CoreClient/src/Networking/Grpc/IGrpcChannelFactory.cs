using Grpc.Core;

namespace Automata.Client.Networking.Grpc
{
    public interface IGrpcChannelFactory
    {
        ChannelBase CreateChannel(GrpcAutomataServer server);
    }
}