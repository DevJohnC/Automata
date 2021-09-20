using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Automata.GrpcServices;
using Grpc.Core;
using Channel = System.Threading.Channels.Channel;
using KindUri = Automata.Kinds.KindUri;

namespace Automata.Events.GrpcServices
{
    public class EventsServiceImpl : EventsService.EventsServiceBase
    {
        private readonly IObserverManager _observerManager;

        public EventsServiceImpl(IObserverManager observerManager)
        {
            _observerManager = observerManager;
        }

        public override async Task ObserveEvents(EventSubscriptionRequest request,
            IServerStreamWriter<ResourceRecord> responseStream,
            ServerCallContext context)
        {
            using var observer = new GrpcEventObserver(_observerManager,
                request.KindUri.NativeKindUri, request.Filter.JsonPathFilters.ToArray());

            while (!context.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    var nextEvent = await observer.ReadNextEvent(
                        context.CancellationToken);

                    await responseStream.WriteAsync(ResourceRecord.FromNative(
                        nextEvent.Serialize()));
                }
                catch (TaskCanceledException)
                {
                }
            }
        }

        private class GrpcEventObserver : IEventObserver, IDisposable
        {
            private readonly IDisposable _subscription;
            private readonly Channel<ResourceDocument> _eventChannel = Channel.CreateUnbounded<ResourceDocument>(
                new UnboundedChannelOptions
                {
                    SingleReader = true,
                    SingleWriter = true
                });

            public GrpcEventObserver(IObserverManager manager,
                KindUri kindUri, string[] eventJsonPathFilters)
            {
                _subscription = manager.AddObserver(this, kindUri, eventJsonPathFilters);
            }
            
            public async Task Next(ResourceDocument eventRecord, CancellationToken cancellationToken)
            {
                await _eventChannel.Writer.WriteAsync(eventRecord, cancellationToken);
            }
            
            public async Task<ResourceDocument> ReadNextEvent(CancellationToken cancellationToken)
            {
                return await _eventChannel.Reader.ReadAsync(cancellationToken);
            }
            
            public void Dispose()
            {
                _subscription.Dispose();
            }
        }
    }
}