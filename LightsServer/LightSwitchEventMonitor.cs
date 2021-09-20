using System.Threading;
using System.Threading.Tasks;
using Automata.Devices;
using LightsShared;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LightsServer
{
    public class LightSwitchEventMonitor : IHostedService
    {
        private readonly DeviceManager _deviceManager;
        private readonly ILightSwitchService _lightSwitchService;
        private readonly ILogger<LightSwitchEventMonitor> _logger;

        public LightSwitchEventMonitor(DeviceManager deviceManager,
            ILightSwitchService lightSwitchService,
            ILogger<LightSwitchEventMonitor> logger)
        {
            _deviceManager = deviceManager;
            _lightSwitchService = lightSwitchService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            HookDeviceEvents();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            UnhookDeviceEvents();
            return Task.CompletedTask;
        }

        private void HookDeviceEvents()
        {
            _lightSwitchService.LightSwitchAdded += LightSwitchServiceOnLightSwitchAdded;
            _lightSwitchService.LightSwitchRemoved += LightSwitchServiceOnLightSwitchRemoved;
            _lightSwitchService.LightSwitchStateChanged += LightSwitchServiceOnLightSwitchStateChanged;
        }
        
        private void UnhookDeviceEvents()
        {
            _lightSwitchService.LightSwitchAdded -= LightSwitchServiceOnLightSwitchAdded;
            _lightSwitchService.LightSwitchRemoved -= LightSwitchServiceOnLightSwitchRemoved;
            _lightSwitchService.LightSwitchStateChanged -= LightSwitchServiceOnLightSwitchStateChanged;
        }

        private Task LightSwitchServiceOnLightSwitchStateChanged(ILightSwitchService sender,
            (LightSwitch LightSwitch, LightSwitchState State) e,
            CancellationToken cancellationToken)
        {
            return _deviceManager.UpdateDeviceState(e.LightSwitch, e.State);
        }

        private Task LightSwitchServiceOnLightSwitchRemoved(ILightSwitchService sender,
            LightSwitch e,
            CancellationToken cancellationToken)
        {
            return _deviceManager.RemoveDevice(e);
        }

        private async Task LightSwitchServiceOnLightSwitchAdded(ILightSwitchService sender,
            LightSwitch e,
            CancellationToken cancellationToken)
        {
            await _deviceManager.AddDevice(e,
                await sender.GetLightSwitchState(e, default),
                sender);
        }
    }
}