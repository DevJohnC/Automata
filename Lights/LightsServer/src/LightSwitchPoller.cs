using System;
using System.Threading;
using System.Threading.Tasks;
using Automata.Devices;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LightsServer
{
    public class LightSwitchPoller : BackgroundService
    {
        private readonly DeviceManager _deviceManager;
        private readonly ILightSwitchService _lightSwitchService;
        private readonly ILogger<LightSwitchPoller> _logger;

        public LightSwitchPoller(ILightSwitchService lightSwitchService,
            ILogger<LightSwitchPoller> logger,
            DeviceManager deviceManager)
        {
            _lightSwitchService = lightSwitchService;
            _logger = logger;
            _deviceManager = deviceManager;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await SyncLightSwitches(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        private async Task SyncLightSwitches(CancellationToken cancellationToken)
        {
            try
            {
                var deviceDelta = _deviceManager.CreateDelta(_lightSwitchService);
                await foreach (var device in _lightSwitchService.GetLightSwitches(cancellationToken))
                {
                    var state = await _lightSwitchService.GetLightSwitchState(device, cancellationToken);
                    deviceDelta.AddDevice(device, state);
                }

                await deviceDelta.Apply();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error syncing light switches");
            }
        }
    }
}