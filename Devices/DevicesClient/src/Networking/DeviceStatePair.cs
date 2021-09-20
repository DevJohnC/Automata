namespace Automata.Devices.Networking
{
    public record DeviceStatePair(
        SerializedResourceDocument DeviceDefinition,
        SerializedResourceDocument DeviceState);
}