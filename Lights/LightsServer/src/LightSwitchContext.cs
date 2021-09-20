/*using System;
using System.Threading;
using System.Threading.Tasks;
using LightsShared;
using ILightSwitchService = LightsServer.ILightSwitchService;
using LightSwitchState = LightsShared.LightSwitchState;

namespace LightsServer
{
    public class LightSwitchContext : DeviceContext<LightsShared.LightSwitch, LightSwitchState>
    {
        private readonly ILightSwitchService _service;

        public LightSwitchContext(ILightSwitchService service)
        {
            _service = service;
        }

        public override async Task Run(CancellationToken stoppingToken)
        {
            _service.LightSwitchAdded += ServiceOnLightSwitchAdded;
            _service.LightSwitchRemoved += ServiceOnLightSwitchRemoved;
            _service.LightSwitchStateChanged += ServiceOnLightSwitchStateChanged;

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    //  start a new set of data
                    BeginNewDelta();
                
                    await foreach (var lightSwitch in _service.GetLightSwitches(stoppingToken))
                    {
                        Devices.Add(lightSwitch);

                        var state = await _service.GetLightSwitchState(lightSwitch, stoppingToken);
                        DeviceStates.SetState(lightSwitch, state);
                    }
                
                    //  any data that was added, existed but wasn't added (or was manually removed)
                    //  or state was updated will now be committed
                    CommitNewDelta();

                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                }
            }
            finally
            {
                _service.LightSwitchAdded -= ServiceOnLightSwitchAdded;
                _service.LightSwitchRemoved -= ServiceOnLightSwitchRemoved;
                _service.LightSwitchStateChanged -= ServiceOnLightSwitchStateChanged;
            }
        }

        private void ServiceOnLightSwitchStateChanged(object? sender, (LightsShared.LightSwitch LightSwitch, LightSwitchState State) e)
        {
            //  todo: should this test case be handled in the DeviceStates collection instead?
            //  so that a SetState call on a missing Device will cause it to be added automatically?
            if (Devices[e.LightSwitch.GetResourceKey()] == null)
            {
                Devices.Add(e.LightSwitch);
            }
            DeviceStates.SetState(e.LightSwitch, e.State);
        }

        private void ServiceOnLightSwitchRemoved(object? sender, LightSwitch e)
        {
            Devices.Remove(e);
        }

        private void ServiceOnLightSwitchAdded(object? sender, LightSwitch e)
        {
            if (Devices[e.GetResourceKey()] == null)
            {
                Devices.Add(e);
            }
        }
    }
}*/