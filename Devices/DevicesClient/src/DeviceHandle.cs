using System;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Kinds;

namespace Automata.Devices
{
    public abstract class DeviceHandle
    {
        internal DeviceHandle(
            IAutomataServer server,
            Guid deviceId,
            DeviceDefinition device,
            Guid stateId,
            DeviceState state)
        {
            Server = server;
            Device = device;
            State = state;
            DeviceId = deviceId;
            StateId = stateId;
        }
        
        public IAutomataServer Server { get; }
        
        public Guid DeviceId { get; }
        
        public DeviceDefinition Device { get; }

        public KindModel DeviceKind => Device.GetKind();
        
        public Guid StateId { get; }
        
        public DeviceState State { get; }

        public KindModel StateKind => State.GetKind();

        protected abstract Task<TrackingDeviceHandle> GetTrackingHandleImpl(CancellationToken cancellationToken);
        
        public Task<TrackingDeviceHandle> GetTrackingHandle(CancellationToken cancellationToken)
        {
            return GetTrackingHandleImpl(cancellationToken);
        }
    }

    public class DeviceHandle<TDevice, TState> : DeviceHandle
        where TDevice : notnull, DeviceDefinition
        where TState : notnull, DeviceState
    {
        public new TDevice Device { get; }
        
        public new TState State { get; }
        
        public DeviceHandle(
            IAutomataServer server,
            ResourceDocument<TDevice> device,
            ResourceDocument<TState> state) :
            base(server,
                device.ResourceId, device.Record,
                state.ResourceId, state.Record)
        {
            Device = device.Record;
            State = state.Record;
        }

        public new async Task<TrackingDeviceHandle<TDevice, TState>> GetTrackingHandle(
            CancellationToken cancellationToken = default)
        {
            var trackingHandle = new TrackingDeviceHandle<TDevice, TState>(
                Server,
                DeviceId,
                Device,
                StateId,
                State);
            await trackingHandle.StartEventsStream(cancellationToken);
            return trackingHandle;
        }

        protected override async Task<TrackingDeviceHandle> GetTrackingHandleImpl(CancellationToken cancellationToken)
        {
            return await GetTrackingHandle(cancellationToken);
        }
    }
}