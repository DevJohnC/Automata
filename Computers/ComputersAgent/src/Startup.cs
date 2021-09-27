using Automata.Devices.GrpcServices;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Automata.Computers.Agent
{
    public class Startup : Automata.HostServer.Startup
    {
        public override void ServicesConfiguring(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.ListenAnyIP(0);
            });
            services.TryAddGrpcService<DeviceServices.DeviceServicesBase, GrpcDeviceControllersService>();
            services.AddDeviceHosting();
            services.AddResourceProvider<LocalComputerManifestProvider>();
        }
    }
}