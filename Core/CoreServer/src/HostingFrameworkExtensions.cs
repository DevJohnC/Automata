using System.Linq;
using Microsoft.AspNetCore.Builder;
using Automata;
using Automata.HostServer.GrpcServices;
using Automata.HostServer.Infrastructure;
using Automata.HostServer.Kinds;
using Automata.Kinds;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HostingFrameworkExtensions
    {
        public static IServiceCollection AddKind<T>(this IServiceCollection services)
            where T : Record
        {
            services.AddTransient(sp => new InstalledKind(
                KindModel.GetKind(typeof(T))));
            return services;
        }
        
        public static IServiceCollection AddResourceProvider<T>(this IServiceCollection services)
            where T : class, IResourceProvider
        {
            services.AddSingleton<IResourceProvider, T>();
            return services;
        }
        
        public static IServiceCollection TryAddGrpcService<TBaseService, TImplementation>(this IServiceCollection services)
            where TBaseService : class
            where TImplementation : class, TBaseService
        {
            if (services.Any(q => q.ServiceType == typeof(GrpcServiceMapper<TBaseService>)))
                return services;

            services.AddTransient<GrpcServiceMapper>(sp =>
                sp.GetRequiredService<GrpcServiceMapper<TBaseService>>());
            services.AddTransient<GrpcServiceMapper<TBaseService>, GrpcServiceMapper<TBaseService>>(
                sp => new GrpcServiceMapper<TBaseService>(
                    endpoints => endpoints.MapGrpcService<TImplementation>()));
            return services;
        }
    }
}