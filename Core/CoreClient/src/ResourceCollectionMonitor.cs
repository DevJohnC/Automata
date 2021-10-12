using System.Threading.Tasks;
using Automata.Client.Networking;
using Automata.Events;

namespace Automata.Client
{
    public class ResourceCollectionMonitor<TResource>
        where TResource : Record
    {
        private readonly Task _runningTask;
        private readonly IAutomataServer _server;

        public ResourceCollectionMonitor(IAutomataServer server)
        {
            _server = server;
            _runningTask = Run();
        }

        private async Task Run()
        {
            //  get current resources via resources service
            //  watch for resource events using a ResourceWatcher
        }
    }
}