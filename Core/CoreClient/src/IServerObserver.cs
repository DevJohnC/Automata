using System.Threading;
using System.Threading.Tasks;
using Automata.Client.Networking;

namespace Automata.Client
{
    public interface IServerObserver
    {
        Task ServerAvailable(IAutomataServer server, CancellationToken cancellationToken);
        
        Task ServerUnavailable(IAutomataServer server, CancellationToken cancellationToken);
    }
}