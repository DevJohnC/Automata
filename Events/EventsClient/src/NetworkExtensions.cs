namespace Automata.Events
{
    public static class NetworkExtensions
    {
        public static GrpcEventsClient CreateEventsClient(
            this Client.GrpcAutomataNetwork network)
        {
            return new GrpcEventsClient(network);
        }
    }
}