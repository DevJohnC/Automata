using System.Threading.Tasks;

namespace Automata.Events
{
    public interface IEventObserver<TEvent>
        where TEvent : notnull, EventRecord
    {
        Task Next(ResourceDocument<TEvent> @event);
    }
}