using System;
using System.Collections.Generic;
using Automata.Kinds;

namespace Automata.Events
{
    public interface IObserverManager
    {
        IReadOnlyList<IEventObserver> GetObservers<TEvent>(TEvent @event)
            where TEvent : EventRecord;
        
        IDisposable AddObserver(IEventObserver observer,
            KindUri eventKindUri,
            string[] eventJsonPathFilters);
    }
}