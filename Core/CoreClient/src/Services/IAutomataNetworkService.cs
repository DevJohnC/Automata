using Automata.Client.Networking;

namespace Automata.Client.Services
{
    public interface IAutomataNetworkService
    {
        IAutomataServer Server { get; }
    }
}