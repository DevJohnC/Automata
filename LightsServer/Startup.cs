using LightsShared;
using Microsoft.Extensions.DependencyInjection;

namespace LightsServer
{
    public class Startup : Automata.HostServer.Startup
    {
        public override void ServicesConfigured(IServiceCollection services)
        {
            services.AddDeviceHosting();
            services.AddDeviceController<LightSwitchController>();
            services.AddKind<LightSwitch>();

            services.AddSingleton<ILightSwitchService, TestLightSwitchService>();
            services.AddHostedService<LightSwitchPoller>();
            services.AddHostedService<LightSwitchEventMonitor>();
        }
    }
}