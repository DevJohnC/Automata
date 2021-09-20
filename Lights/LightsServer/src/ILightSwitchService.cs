using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Automata;
using LightsShared;

namespace LightsServer
{
    public interface ILightSwitchService
    {
        event AsyncEventHandler<ILightSwitchService, LightSwitch> LightSwitchAdded;
        event AsyncEventHandler<ILightSwitchService, LightSwitch> LightSwitchRemoved;
        event AsyncEventHandler<ILightSwitchService, (LightSwitch LightSwitch, LightSwitchState State)> LightSwitchStateChanged;

        IAsyncEnumerable<LightSwitch> GetLightSwitches(CancellationToken cancellationToken);

        Task<LightSwitchState> GetLightSwitchState(LightSwitch lightSwitch,
            CancellationToken cancellationToken);

        Task TurnOn(LightSwitch lightSwitch);
        
        Task TurnOff(LightSwitch lightSwitch);

        Task SetPowerLevel(LightSwitch lightSwitch, double powerLevel);
    }
}