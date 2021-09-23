namespace Automata.Devices
{
    public record DeviceStatePair(
        SerializedResourceDocument DeviceDefinition,
        SerializedResourceDocument DeviceState);
}