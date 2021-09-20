using Automata.Kinds;

namespace Automata.Devices
{
    [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "state", "states")]
    public abstract record DeviceState : Record;
}