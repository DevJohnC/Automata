using System;
using System.Collections.Generic;
using Automata.Devices;
using Automata.Kinds;

namespace Automata.Computers
{
    [Kind(Strings.KindGroup, Strings.CurrentApiVersion, "manifest", "manifests")]
    public record ComputerManifest(
        Guid ComputerId,
        OperatingSystem OperatingSystem,
        string Hostname,
        DateTime StartupTimeUtc,
        List<string> PhysicalAddresses) : Record;
    
    [Kind(Strings.KindGroup, Strings.CurrentApiVersion, "computer", "computers")]
    public record Computer(
        OperatingSystem OperatingSystem) : DeviceDefinition;

    [Kind(Strings.KindGroup, Strings.CurrentApiVersion, "computerState", "computerStates")]
    public record ComputerState(
        string Hostname,
        DateTime StartupTimeUtc,
        AvailabilityState AvailabilityState,
        List<string> PhysicalAddresses) : DeviceState;

    public enum OperatingSystem
    {
        Windows,
        Linux,
        MacOS
    }

    public enum AvailabilityState
    {
        Up,
        Down
    }
}