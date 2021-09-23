using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Events.GrpcServices;
using Automata.GrpcServices;
using Automata.Kinds;
using Grpc.Core;
using KindUri = Automata.Kinds.KindUri;
using PluralKindUri = Automata.GrpcServices.PluralKindUri;

namespace Automata.Events
{
    public class GrpcEventsClient : IEventsClient
    {
        public static IEventsClient Factory(GrpcAutomataServer server)
        {
            return new GrpcEventsClient(
                server.ChannelFactory.CreateChannel(server),
                server);
        }
        
        private readonly EventsService.EventsServiceClient _client;

        private readonly GrpcAutomataServer _server;

        public IAutomataServer Server => _server;

        public GrpcEventsClient(ChannelBase channelBase, GrpcAutomataServer server)
        {
            _server = server;
            _client = new EventsService.EventsServiceClient(channelBase);
        }

        public async Task<IAsyncEnumerable<SerializedResourceDocument>> ObserveEvents(
            KindUri eventKindUri,
            CancellationToken requestCancellationToken = default,
            params string[] jsonPathFilter)
        {
            var filter = new EventSubscriptionFilter();
            filter.JsonPathFilters.AddRange(jsonPathFilter);
            var streamingCall = _client.ObserveEvents(new()
            {
                KindUri = Automata.GrpcServices.KindUri.FromNative(eventKindUri),
                Filter = filter
            }, cancellationToken: requestCancellationToken);
            //  todo: await for the round-trip to establish the events observer
            //await streamingCall.ResponseHeadersAsync;
            
            return Impl();

            async IAsyncEnumerable<SerializedResourceDocument> Impl(
                [EnumeratorCancellation] CancellationToken stoppingToken = default)
            {
                using (streamingCall)
                {
                    while (await streamingCall.ResponseStream.MoveNext(stoppingToken))
                    {
                        stoppingToken.ThrowIfCancellationRequested();
                        yield return streamingCall.ResponseStream.Current.ToNative();
                    }
                }
            }
        }
    }
}