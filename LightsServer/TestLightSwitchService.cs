using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Automata;
using LightsShared;

namespace LightsServer
{
    internal class TestLightSwitchService : ILightSwitchService
    {
        private readonly Dictionary<LightSwitch, LightSwitchState> _lightSwitches = new()
        {
            {new LightSwitch("TestLight"), new LightSwitchState(LightSwitchPowerState.Off, 0.5)}
        };

        public event AsyncEventHandler<ILightSwitchService, LightSwitch>? LightSwitchAdded;
        public event AsyncEventHandler<ILightSwitchService, LightSwitch>? LightSwitchRemoved;
        public event AsyncEventHandler<ILightSwitchService, (LightSwitch LightSwitch, LightSwitchState State)>? LightSwitchStateChanged;

        public async IAsyncEnumerable<LightSwitch> GetLightSwitches(CancellationToken cancellationToken)
        {
            foreach (var kvp in _lightSwitches)
            {
                yield return kvp.Key;
            }
        }

        public Task<LightSwitchState> GetLightSwitchState(LightSwitch lightSwitch, CancellationToken cancellationToken)
        {
            return Task.FromResult(_lightSwitches[lightSwitch]);
        }

        public Task TurnOn(LightSwitch lightSwitch)
        {
            _lightSwitches[lightSwitch] = _lightSwitches[lightSwitch]
                with
                {
                    PowerState = LightSwitchPowerState.On
                };
            return LightSwitchStateChanged?.SerialInvoke(this, (lightSwitch, _lightSwitches[lightSwitch]))
                ?? Task.CompletedTask;
        }

        public Task TurnOff(LightSwitch lightSwitch)
        {
            _lightSwitches[lightSwitch] = _lightSwitches[lightSwitch]
                with
                {
                    PowerState = LightSwitchPowerState.Off
                };
            return LightSwitchStateChanged?.SerialInvoke(this, (lightSwitch, _lightSwitches[lightSwitch]))
                   ?? Task.CompletedTask;
        }

        public Task SetPowerLevel(LightSwitch lightSwitch, double powerLevel)
        {
            _lightSwitches[lightSwitch] = _lightSwitches[lightSwitch]
                with
                {
                    PowerLevel = powerLevel
                };
            return LightSwitchStateChanged?.SerialInvoke(this, (lightSwitch, _lightSwitches[lightSwitch]))
                   ?? Task.CompletedTask;
        }
    }
}