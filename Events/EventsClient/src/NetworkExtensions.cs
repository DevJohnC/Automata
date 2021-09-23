using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Kinds;

namespace Automata.Events
{
    public static class NetworkExtensions
    {
        public static Task<IAsyncDisposable> AddObserver<TEvent>(
            this AutomataNetwork network,
            IEventObserver<TEvent> observer,
            params string[] jsonPathFilter)
            where TEvent : EventRecord
        {
            return network.AddObserver(observer, default, jsonPathFilter);
        }
        
        public static async Task<IAsyncDisposable> AddObserver<TEvent>(
            this AutomataNetwork network,
            IEventObserver<TEvent> observer,
            CancellationToken cancellationToken,
            params string[] jsonPathFilter)
            where TEvent : EventRecord
        {
            var disposables = new List<IAsyncDisposable>();
            foreach (var server in network.Servers)
            {
                try
                {
                    disposables.Add(await network.AddObserver(
                        server, observer, cancellationToken, jsonPathFilter));
                }
                catch
                {
                    //  safely dispose
                    await using var collection = new ObserverCancellationCollection(disposables);

                    throw;
                }
            }
            
            return new ObserverCancellationCollection(disposables);
        }
        
        public static Task<IAsyncDisposable> AddObserver<TEvent>(
            this AutomataNetwork network,
            IAutomataServer server,
            IEventObserver<TEvent> observer,
            CancellationToken cancellationToken,
            params string[] jsonPathFilter)
            where TEvent : EventRecord
        {
            return network.AddObserver(
                server.CreateService<IEventsClient>(),
                observer,
                cancellationToken,
                jsonPathFilter);
        }
        
        public static async Task<IAsyncDisposable> AddObserver<TEvent>(
            this AutomataNetwork network,
            IEventsClient client,
            IEventObserver<TEvent> observer,
            CancellationToken cancellationToken,
            params string[] jsonPathFilter)
            where TEvent : EventRecord
        {
            return new ObserverCancellation<TEvent>(observer, await client.ObserveEvents(
                KindModel.GetKind(typeof(TEvent)).Name.PluralUri,
                cancellationToken,
                jsonPathFilter));
        }

        private class ObserverCancellation<TEvent> : IAsyncDisposable
            where TEvent : EventRecord
        {
            private readonly IEventObserver<TEvent> _observer;
            private readonly IAsyncEnumerable<SerializedResourceDocument> _eventsStream;
            private readonly Task _runTask;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

            public ObserverCancellation(
                IEventObserver<TEvent> observer,
                IAsyncEnumerable<SerializedResourceDocument> eventsStream)
            {
                _observer = observer;
                _eventsStream = eventsStream;
                _runTask = Run(_cancellation.Token);
            }

            private async Task Run(CancellationToken stoppingToken)
            {
                await foreach (var serializedEventDocument in _eventsStream
                    .WithCancellation(stoppingToken))
                {
                    var @event = serializedEventDocument
                        .Deserialize<TEvent>();
                    //  todo: consider propagating the stopping token here
                    await _observer.Next(@event);
                }
            }
            
            public async ValueTask DisposeAsync()
            {
                _cancellation.Cancel();
                try
                {
                    await _runTask;
                }
                catch (Exception e)
                {
                    //  todo: properly log
                    Console.WriteLine(e);
                }
                _cancellation.Dispose();
            }
        }

        private class ObserverCancellationCollection : IAsyncDisposable
        {
            private readonly IReadOnlyList<IAsyncDisposable> _disposables;

            public ObserverCancellationCollection(IReadOnlyList<IAsyncDisposable> disposables)
            {
                _disposables = disposables;
            }
            
            public async ValueTask DisposeAsync()
            {
                foreach (var disposable in _disposables)
                {
                    try
                    {
                        await disposable.DisposeAsync();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }
    }
}