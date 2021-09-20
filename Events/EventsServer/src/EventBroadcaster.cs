using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Automata.Events
{
    public class EventBroadcaster : IEventBroadcaster
    {
        private readonly IObserverManager _observerManager;

        public EventBroadcaster(IObserverManager observerManager)
        {
            _observerManager = observerManager;
        }

        public Task BroadcastEvent<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
            where TEvent : EventRecord
        {
            var observers = _observerManager.GetObservers(@event);
            if (observers.Count == 0)
                return Task.CompletedTask;

            var eventDocument = new ResourceDocument<TEvent>(
                Guid.NewGuid(), @event);

            var tasks = new List<Task>(observers.Count);
            foreach (var observer in observers)
            {
                tasks.Add(observer.Next(eventDocument, cancellationToken));
            }

            return Task.WhenAll(tasks);
        }
    }
}