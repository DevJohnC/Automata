using System;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Events;

namespace Automata.Devices
{
    public abstract class TrackingDeviceHandle : IAsyncDisposable
    {
        public event AsyncEventHandler<TrackingDeviceHandle, EventArgs>? StateChanged;
        
        protected readonly Guid StateId;
        protected DeviceState StateSnapshot;
        
        public AutomataNetwork Network { get; }
        
        public IAutomataServer Server { get; }
        
        public Guid DeviceId { get; }
        
        public DeviceDefinition Device { get; }

        internal TrackingDeviceHandle(
            AutomataNetwork network,
            IAutomataServer server,
            Guid deviceId,
            DeviceDefinition device,
            Guid stateId,
            DeviceState stateSnapshot)
        {
            Network = network;
            StateId = stateId;
            StateSnapshot = stateSnapshot;
            Server = server;
            DeviceId = deviceId;
            Device = device;
        }

        protected abstract ResourceDocument GetStateSnapshotImpl();

        protected Task OnStateChanged()
        {
            return StateChanged?.SerialInvoke(this, EventArgs.Empty)
                ?? Task.CompletedTask;
        }

        public ResourceDocument GetStateSnapshot()
        {
            return GetStateSnapshotImpl();
        }

        public abstract ValueTask DisposeAsync();
    }

    public class TrackingDeviceHandle<TDevice, TState> : TrackingDeviceHandle
        where TDevice : DeviceDefinition
        where TState : DeviceState
    {
        private readonly StateEventObserver _stateObserver;
        private IAsyncDisposable? _observerCancellation;
        public new TDevice Device { get; }

        public TrackingDeviceHandle(
            AutomataNetwork network,
            IAutomataServer server,
            Guid deviceId,
            TDevice device,
            Guid stateId,
            TState stateSnapshot) :
            base(network, server, deviceId, device, stateId, stateSnapshot)
        {
            Device = device;
            _stateObserver = new StateEventObserver(this);
        }

        private Task UpdateSnapshot(TState snapshot)
        {
            StateSnapshot = snapshot;
            return OnStateChanged();
        }

        internal async Task StartEventsStream(CancellationToken cancellationToken)
        {
            _observerCancellation = await Network.AddObserver(
                Server,
                _stateObserver,
                cancellationToken,
                $"$[?(@.deviceId == '{DeviceId.ToString().ToLowerInvariant()}')]");
        }

        protected override ResourceDocument GetStateSnapshotImpl()
        {
            return GetStateSnapshot();
        }

        public new ResourceDocument<TState> GetStateSnapshot()
        {
            return new ResourceDocument<TState>(StateId, (TState)StateSnapshot);
        }

        public override ValueTask DisposeAsync()
        {
            return _observerCancellation?.DisposeAsync() ??
                   default;
        }

        private class StateEventObserver : IEventObserver<Events.DeviceStateChangedEvent>
        {
            private TrackingDeviceHandle<TDevice, TState> _parent;

            public StateEventObserver(TrackingDeviceHandle<TDevice, TState> parent)
            {
                _parent = parent;
            }

            public Task Next(ResourceDocument<Events.DeviceStateChangedEvent> @event)
            {
                var newState = @event.Record.State.Deserialize<TState>();
                _parent.UpdateSnapshot(newState.Record);
                return Task.CompletedTask;
            }
        }
    }
}