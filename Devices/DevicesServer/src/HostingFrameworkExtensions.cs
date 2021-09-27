using System.Linq;
using Automata.Devices;
using Automata.Devices.GrpcServices;
using Automata.HostServer.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HostingFrameworkExtensions
    {
        public static IServiceCollection AddDeviceHosting(this IServiceCollection services)
        {
            services.AddEvents();

            if (services.All(q => q.ServiceType != typeof(DeviceManager)))
            {
                services.AddKind<DeviceDefinition>();
                services.AddKind<DeviceState>();
                services.AddKind<DeviceController>();
                services.AddKind<DeviceControlRequest>();

                services.AddSingleton<DeviceControllerMetadataProvider>();
                
                services.AddSingleton<DeviceManager>();
                services.AddSingleton<IResourceProvider>(sp => sp.GetRequiredService<DeviceManager>());

                services.AddSingleton<DeviceControllerManager>();
                services.AddSingleton<IResourceProvider>(sp => sp.GetRequiredService<DeviceControllerManager>());
                
                services.TryAddGrpcService<DeviceServices.DeviceServicesBase, DevicesServiceImpl>();
            }

            return services;
        }

        public static IServiceCollection AddDeviceController<T>(this IServiceCollection services)
            where T : class, IDeviceController
        {
            return services
                .AddDeviceHosting()
                .AddSingleton<IDeviceController, T>();
        }
        
        public static IServiceCollection AddDeviceController(this IServiceCollection services, IDeviceController instance)
        {
            return services
                .AddDeviceHosting()
                .AddSingleton<IDeviceController>(instance);
        }
    }
}