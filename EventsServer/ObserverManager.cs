using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Automata.Kinds;
using Newtonsoft.Json.Linq;

namespace Automata.Events
{
    public class ObserverManager : IObserverManager
    {
        private readonly ReaderWriterLockSlim _lock = new();
        private readonly List<ObserverRecord> _observers = new();
        
        public IReadOnlyList<IEventObserver> GetObservers<TEvent>(TEvent @event)
            where TEvent : EventRecord
        {
            using var readLock = _lock.UseReadLock();

            var kind = KindModel.GetKind(typeof(TEvent));
            var eventJson = ResourceSerializer.Serialize(@event);
            //  an array is used to facilitate the selectors in jsonpath
            var jsonArray = new JArray { eventJson };

            return _observers
                .Where(q => q.Matches(kind, jsonArray))
                .Select(q => q.Observer)
                .ToList();
        }

        public IDisposable AddObserver(IEventObserver observer, KindUri eventKindUri, string[] eventJsonPathFilters)
        {
            var record = new ObserverRecord(observer, eventKindUri, eventJsonPathFilters);

            using var writeLock = _lock.UseWriteLock();
            _observers.Add(record);

            return new Subscription(record, this);
        }

        private void RemoveObserver(ObserverRecord observerRecord)
        {
            using var writeLock = _lock.UseWriteLock();
            _observers.Remove(observerRecord);
        }

        private class Subscription : IDisposable
        {
            private readonly ObserverRecord _observerRecord;
            private readonly ObserverManager _observerManager;

            public Subscription(ObserverRecord observerRecord,
                ObserverManager observerManager)
            {
                _observerRecord = observerRecord;
                _observerManager = observerManager;
            }

            public void Dispose()
            {
                _observerManager.RemoveObserver(_observerRecord);
            }
        }

        private record ObserverRecord(IEventObserver Observer, KindUri EventKindUri, string[] JsonPathFilters)
        {
            public bool Matches(KindModel kind, JArray eventData)
            {
                if (!kind.Name.MatchesUri(EventKindUri))
                    return false;

                foreach (var pathFilters in JsonPathFilters)
                {
                    var token = eventData.SelectToken(pathFilters, false);
                    if (token == null)
                        return false;
                }
                
                return true;
            }
        }
    }
}