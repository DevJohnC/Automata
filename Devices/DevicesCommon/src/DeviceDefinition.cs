using Automata.Kinds;

namespace Automata.Devices
{
    [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "device", "devices")]
    public abstract record DeviceDefinition : Record;
}