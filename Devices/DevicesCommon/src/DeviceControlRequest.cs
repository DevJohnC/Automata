using Automata.Kinds;

namespace Automata.Devices
{
    [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "deviceControlRequest", "deviceControlRequests")]
    public record DeviceControlRequest : Record;
}