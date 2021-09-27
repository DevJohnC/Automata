using Automata.Client;
using Automata.Client.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Automata.Computers.Server
{
    public class Startup : Automata.HostServer.Startup
    {
        public override void ServicesConfiguring(IServiceCollection services)
        {
            services.AddDeviceHosting();
            services.AddDeviceController<ComputerController>();
            
            services.Configure<KestrelServerOptions>(options =>
            {
                options.ListenAnyIP(0);
            });

            services.AddSingleton(new AutomataNetwork());
            services.AddSingleton<IServerLocator>(new SsdpServerLocator());
            services.AddSingleton(new ServerServiceProvider<GrpcAutomataServer>());

            services.AddHostedService<ComputerMonitoringService>();
        }
    }
}