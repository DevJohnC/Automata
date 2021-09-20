using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Events.GrpcServices;
using Automata.GrpcServices;
using Automata.Kinds;
using Grpc.Core;
using PluralKindUri = Automata.GrpcServices.PluralKindUri;

namespace Automata.Events
{
    public class GrpcEventsClient
    {
        public GrpcAutomataNetwork Network { get; }

        public GrpcEventsClient(GrpcAutomataNetwork network)
        {
            Network = network;
        }

        public Task<IAsyncDisposable> AddObserver<TEvent>(
            IEventObserver<TEvent> observer,
            params string[] jsonPathFilter)
            where TEvent : EventRecord
        {
            return AddObserver(observer, default, jsonPathFilter);
        }
        
        public async Task<IAsyncDisposable> AddObserver<TEvent>(
            IEventObserver<TEvent> observer,
            CancellationToken cancellationToken,
            params string[] jsonPathFilter)
            where TEvent : EventRecord
        {
            var disposables = new List<IAsyncDisposable>();
            foreach (var server in Network.Servers)
            {
                try
                {
                    disposables.Add(await AddObserver(server, observer, cancellationToken, jsonPathFilter));
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
        
        public Task<IAsyncDisposable> AddObserver<TEvent>(
            GrpcAutomataServer server,
            IEventObserver<TEvent> observer,
            params string[] jsonPathFilter)
            where TEvent : EventRecord
        {
            return AddObserver(server, observer, default, jsonPathFilter);
        }
        
        public async Task<IAsyncDisposable> AddObserver<TEvent>(
            GrpcAutomataServer server,
            IEventObserver<TEvent> observer,
            CancellationToken cancellationToken,
            params string[] jsonPathFilter)
            where TEvent : EventRecord
        {
            var client = new EventsService.EventsServiceClient(
                server.ChannelFactory.CreateChannel(server));
            var filter = new EventSubscriptionFilter();
            filter.JsonPathFilters.AddRange(jsonPathFilter);
            var streamingCall = client.ObserveEvents(new()
            {
                KindUri = new()
                {
                    PluralUri = PluralKindUri.FromKindName(KindModel.GetKind(typeof(TEvent)).Name)
                },
                Filter = filter
            }, cancellationToken: cancellationToken);
            
            return new ObserverCancellation<TEvent>(observer, streamingCall);
        }

        private class ObserverCancellation<TEvent> : IAsyncDisposable
            where TEvent : EventRecord
        {
            private readonly IEventObserver<TEvent> _observer;
            private readonly AsyncServerStreamingCall<ResourceRecord> _streamingCall;
            private readonly Task _runTask;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

            public ObserverCancellation(
                IEventObserver<TEvent> observer,
                AsyncServerStreamingCall<ResourceRecord> streamingCall)
            {
                _observer = observer;
                _streamingCall = streamingCall;
                _runTask = Run(_cancellation.Token);
            }

            private async Task Run(CancellationToken stoppingToken)
            {
                while (await _streamingCall.ResponseStream.MoveNext(stoppingToken))
                {
                    var @event = _streamingCall.ResponseStream.Current
                        .ToResourceDocument()
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
                _streamingCall.Dispose();
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