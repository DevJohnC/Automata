using System;

namespace Automata.Devices
{
    public record DeviceSnapshot(
        ResourceDocument<DeviceDefinition> Device,
        ResourceDocument<DeviceState> State,
        DateTime SnapshotTime);

    public record DeviceSnapshot<TDevice, TState> : DeviceSnapshot
        where TDevice : DeviceDefinition
        where TState : DeviceState
    {
        public new ResourceDocument<TDevice> Device { get; }
        public new ResourceDocument<TState> State { get; }

        public DeviceSnapshot(
            ResourceDocument<TDevice> device,
            ResourceDocument<TState> state,
            DateTime snapshotTime) :
            base(
                new(device.ResourceId, device.Record),
                new (state.ResourceId, state.Record),
                snapshotTime)
        {
            Device = device;
            State = state;
        }
    }
}