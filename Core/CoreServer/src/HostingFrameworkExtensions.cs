using System.Linq;
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
        
        public static IServiceCollection TryAddGrpcService<TService>(this IServiceCollection services)
            where TService : class
        {
            if (services.Any(q => q.ImplementationType == typeof(GrpcServiceMapper<TService>)))
                return services;
            
            services.AddTransient<GrpcServiceMapper, GrpcServiceMapper<TService>>();
            return services;
        }
    }
}