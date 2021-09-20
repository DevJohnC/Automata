using Automata.Devices;
using Automata.Kinds;

namespace LightsShared
{
    [Kind("lighting", "v1", "lightSwitch", "lightSwitches")]
    public record LightSwitch(string NetworkId) : DeviceDefinition;

    [Kind("lighting", "v1", "lightSwitchState", "lightSwitchStates")]
    public record LightSwitchState(
            LightSwitchPowerState PowerState,
            double PowerLevel)
        : DeviceState;

    public enum LightSwitchPowerState
    {
        On,
        Off
    }
}