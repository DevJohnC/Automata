using System.Threading;
using System.Threading.Tasks;

namespace Automata.Events
{
    public interface IEventBroadcaster
    {
        Task BroadcastEvent<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : EventRecord;
    }
}