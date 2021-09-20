using Automata.Devices;
using Automata.Kinds;

namespace LightsShared
{
    [Kind("lighting", "v1", "setPowerLevelRequest", "setPowerLevelRequests")]
    public record SetPowerLevel(double PowerLevel) : DeviceControlRequest;

    [Kind("lighting", "v1", "setPowerStateRequest", "setPowerStateRequests")]
    public record SetPowerState(LightSwitchPowerState PowerState) : DeviceControlRequest;
}