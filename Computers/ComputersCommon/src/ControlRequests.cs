using Automata.Devices;
using Automata.Kinds;

namespace Automata.Computers
{
    [Kind(Strings.KindGroup, Strings.CurrentApiVersion, "wakeUpRequest", "wakeUpRequests")]
    public record WakeUp : DeviceControlRequest;

    [Kind(Strings.KindGroup, Strings.CurrentApiVersion, "turnOffRequest", "turnOffRequests")]
    public record TurnOff(
        OffPowerState OffPowerState) : DeviceControlRequest;

    public enum OffPowerState
    {
        Sleep,
        Hibernate,
        PowerOff
    }
}