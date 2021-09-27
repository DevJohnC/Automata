using Automata.Events;
using Automata.Events.GrpcServices;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HostingFrameworkExtensions
    {
        public static IServiceCollection AddEvents(this IServiceCollection services)
        {
            services.AddKind<EventRecord>();
            services.TryAddSingleton<IEventBroadcaster, EventBroadcaster>();
            services.TryAddSingleton<IObserverManager, ObserverManager>();
            services.TryAddGrpcService<EventsService.EventsServiceBase, EventsServiceImpl>();
            return services;
        }
    }
}