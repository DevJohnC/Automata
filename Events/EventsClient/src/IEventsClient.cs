using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Kinds;

namespace Automata.Events
{
    public interface IEventsClient : IAutomataNetworkService
    {
        Task<IAsyncEnumerable<SerializedResourceDocument>> ObserveEvents(
            KindUri eventKindUri,
            CancellationToken requestCancellationToken = default,
            params string[] jsonPathFilter);
    }
}