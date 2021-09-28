using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.Events;
using Automata.HostServer.Infrastructure;
using Automata.HostServer.Resources;
using Automata.Kinds;
using Newtonsoft.Json;

namespace Automata.Devices
{
    public class DeviceManager : IResourceProvider
    {
        private readonly IEventBroadcaster _events;
        private readonly IResourceIdPersistence _resourceIdPersistence;

        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);
        private readonly Dictionary<Guid, DeviceRecord> _devices = new();
        private readonly ConditionalWeakTable<object, Dictionary<Guid, DeviceRecord>> _ownedDevices = new();

        public DeviceManager(IEventBroadcaster events,
            IResourceIdPersistence resourceIdPersistence)
        {
            _events = events;
            _resourceIdPersistence = resourceIdPersistence;
        }
        
        async IAsyncEnumerable<ResourceDocument> IResourceProvider.GetResources()
        {
            using var readLock = _lock.UseReadLock();
            foreach (var kvp in _devices)
            {
                yield return kvp.Value.Device;
                yield return kvp.Value.State;
            }
        }

        async IAsyncEnumerable<ResourceDocument> IResourceProvider.GetAssociatedResources(
            ResourceIdentifier resourceIdentifier)
        {
            using var readLock = _lock.UseReadLock();
            if (_devices.TryGetValue(resourceIdentifier.ResourceId, out var deviceRecord))
            {
                yield return deviceRecord.State;
            }
        }

        public async IAsyncEnumerable<DeviceSnapshot> GetSnapshots(KindUri deviceKindUri, KindUri stateKindUri)
        {
            using var readLock = _lock.UseReadLock();
            foreach (var kvp in _devices
                .Where(q => q.Value.Matches(deviceKindUri, stateKindUri)))
            {
                yield return kvp.Value.GetSnapshot();
            }
        }

        public async IAsyncEnumerable<ResourceDocument> GetDevices(KindUri deviceKindUri)
        {
            using var readLock = _lock.UseReadLock();
            foreach (var kvp in _devices
                .Where(q => q.Value.Matches(deviceKindUri)))
            {
                yield return kvp.Value.Device;
            }
        }
        
        public async IAsyncEnumerable<ResourceDocument<T>> GetDevices<T>()
            where T : DeviceDefinition
        {
            var deviceKind = KindModel.GetKind(typeof(T));
            using var readLock = _lock.UseReadLock();
            foreach (var kvp in _devices
                .Where(q => q.Value.Matches(deviceKind.Name.SingularUri)))
            {
                if (kvp.Value.Device is ResourceDocument<T> deviceDocument)
                {
                    yield return deviceDocument;
                }
            }
        }

        private Task<Guid> GetDeviceId<TDevice>(TDevice device)
            where TDevice : Record
        {
            return _resourceIdPersistence.GetOrCreateResourceIdAsync(device);
        }

        private Task<Guid> GetStateId(Guid deviceId, KindName stateKindName)
        {
            var stateRecord = new StateRecord(deviceId, stateKindName);
            return _resourceIdPersistence.GetOrCreateResourceIdAsync(stateRecord);
        }

        private async Task<(Guid DeviceId, Guid StateId)> GetResourceIds<TDevice>(
            TDevice device,
            KindName stateKindName)
            where TDevice : Record
        {
            var deviceId = await GetDeviceId(device);
            var stateRecord = new StateRecord(deviceId, stateKindName);
            var stateId = await _resourceIdPersistence.GetOrCreateResourceIdAsync(stateRecord);

            return (deviceId, stateId);
        }

        public async Task<TryResult<TDevice>> TryGetDevice<TDevice>(Guid deviceId)
            where TDevice : DeviceDefinition
        {
            using var readLock = _lock.UseReadLock();

            if (!_devices.TryGetValue(deviceId, out var deviceRecord) ||
                deviceRecord.Device.Record is not TDevice device)
                return TryResult<TDevice>.Failure;
            
            return TryResult<TDevice>.Success(device);
        }
        
        public async Task<bool> TryAddDevice<TDevice, TState>(TDevice device,
            TState state,
            object? owner = null)
            where TDevice : DeviceDefinition
            where TState : DeviceState
        {
            using var writeLock = _lock.UseWriteLock();

            var (deviceId, stateId) = await GetResourceIds(device, state.GetKind().Name);
            var record = new DeviceRecord<TDevice, TState>(
                new ResourceDocument<TDevice>(deviceId, device),
                new ResourceDocument<TState>(stateId, state),
                owner);
            
            if (!_devices.TryAdd(deviceId, record))
                return false;

            if (owner != null)
            {
                AddOwnedDevice(owner, record);
            }

            //await _events.BroadcastEvent(new Events.DeviceAddedEvent());
            return true;
        }

        public async Task AddDevice<TDevice, TState>(TDevice device,
            TState state,
            object? owner = null)
            where TDevice : DeviceDefinition
            where TState : DeviceState
        {
            using var writeLock = _lock.UseWriteLock();

            var (deviceId, stateId) = await GetResourceIds(device, state.GetKind().Name);
            var record = new DeviceRecord<TDevice, TState>(
                new ResourceDocument<TDevice>(deviceId, device),
                new ResourceDocument<TState>(stateId, state),
                owner);
            _devices.Add(deviceId, record);

            if (owner != null)
            {
                AddOwnedDevice(owner, record);
            }
            //await _events.BroadcastEvent(new Events.DeviceAddedEvent());
        }
        
        public async Task AddDevice<TDevice, TState>(
            ResourceDocument<TDevice> device,
            TState state,
            object? owner = null)
            where TDevice : DeviceDefinition
            where TState : DeviceState
        {
            using var writeLock = _lock.UseWriteLock();

            var stateId = await GetStateId(device.ResourceId, state.GetKind().Name);
            var record = new DeviceRecord<TDevice, TState>(
                device,
                new ResourceDocument<TState>(stateId, state),
                owner);
            _devices.Add(device.ResourceId, record);

            if (owner != null)
            {
                AddOwnedDevice(owner, record);
            }
            //await _events.BroadcastEvent(new Events.DeviceAddedEvent());
        }

        private void AddOwnedDevice<TDevice, TState>(object owner, DeviceRecord<TDevice, TState> record)
            where TDevice : DeviceDefinition where TState : DeviceState
        {
            if (!_ownedDevices.TryGetValue(owner, out var ownedDevices))
            {
                ownedDevices = new();
                _ownedDevices.Add(owner, ownedDevices);
            }

            ownedDevices.Add(record.Device.ResourceId, record);
        }

        private bool AreStatesDifferent<TState>(TState left, TState right)
            where TState : Record
        {
            var leftJson = ResourceSerializer.Serialize(left).ToString(Formatting.None);
            var rightJson = ResourceSerializer.Serialize(right).ToString(Formatting.None);

            return leftJson != rightJson;
        }

        public async Task UpdateDeviceState<TDevice, TState>(TDevice device,
            TState state,
            bool force = false)
            where TDevice : DeviceDefinition
            where TState : DeviceState
        {
            using var readLock = _lock.UseReadLock();
            
            var (deviceId, stateId) = await GetResourceIds(device, state.GetKind().Name);
            if (!_devices.TryGetValue(deviceId, out var record) ||
                record is not DeviceRecord<TDevice, TState> typedRecord)
                return;

            if (!(force || AreStatesDifferent(state, record.State.Record)))
                return;

            var newState = new ResourceDocument<TState>(stateId, state);
            typedRecord.Update(newState);
            await _events.BroadcastEvent(Events.DeviceStateChangedEvent.Create(
                new ResourceDocument<TDevice>(deviceId, device),
                new ResourceDocument<TState>(stateId, state)));
        }

        public async Task RemoveDevice<TDevice>(TDevice device)
            where TDevice : DeviceDefinition
        {
            using var readLock = _lock.UseUpgradableReadLock();

            var deviceId = await GetDeviceId(device);
            if (!_devices.TryGetValue(deviceId, out var record))
                return;

            using var writeLock = _lock.UseWriteLock();

            _devices.Remove(deviceId);

            if (record.Owner != null &&
                _ownedDevices.TryGetValue(record.Owner, out var ownedDevices))
            {
                ownedDevices.Remove(deviceId);
            }

            //await _events.BroadcastEvent(new Events.DeviceRemovedEvent());
        }

        public DeviceDelta CreateDelta(object owner)
        {
            using var readLock = _lock.UseReadLock();
            
            if (!_ownedDevices.TryGetValue(owner, out var records))
                return new DeviceDelta(this, owner,
                    new());
            
            return new DeviceDelta(
                this,
                owner,
                records
                    .Select(q => q.Value.CreatePendingRemoval())
                    .ToList());
        }

        private record StateRecord(Guid DeviceId, KindName StateKindName) : Record;

        private abstract class DeviceRecord
        {
            protected DeviceRecord(
                KindModel deviceKind,
                KindModel stateKind,
                ResourceDocument device,
                object? owner)
            {
                _deviceKind = deviceKind;
                _stateKind = stateKind;
                Device = device;
                Owner = owner;
            }

            private readonly KindModel _deviceKind;

            private readonly KindModel _stateKind;
            
            public ResourceDocument Device { get; }
            
            public abstract ResourceDocument State { get; }
            public object? Owner { get; }

            public bool Matches(KindUri deviceUri)
            {
                return _deviceKind.Name.MatchesUri(deviceUri);
            }

            public bool Matches(KindUri deviceUri, KindUri stateUri)
            {
                return _deviceKind.Name.MatchesUri(deviceUri) &&
                       _stateKind.Name.MatchesUri(stateUri);
            }
            
            public abstract DeviceSnapshot GetSnapshot();

            public abstract DeviceDelta.PendingRemoval CreatePendingRemoval();
        }

        private class DeviceRecord<TDevice, TState> : DeviceRecord
            where TDevice : DeviceDefinition
            where TState : DeviceState
        {
            private readonly ResourceDocument<TDevice> _device;
            private ResourceDocument<TState> _state;
            private DateTime _createdTime = DateTime.Now;

            public override ResourceDocument State => _state;

            public DeviceRecord(
                ResourceDocument<TDevice> device,
                ResourceDocument<TState> state,
                object? owner) :
                base(device.Record.GetKind(), state.Record.GetKind(), device, owner)
            {
                _device = device;
                _state = state;
            }

            public void Update(ResourceDocument<TState> newState)
            {
                _state = newState;
                _createdTime = DateTime.Now;
            }

            public override DeviceSnapshot GetSnapshot()
            {
                return new DeviceSnapshot<TDevice, TState>(_device, _state, _createdTime);
            }

            public override DeviceDelta.PendingRemoval CreatePendingRemoval()
            {
                return new PendingRemoval<TDevice>(_device.Record);
            }
        }
        
        private class PendingRemoval<TDevice> : DeviceDelta.PendingRemoval
            where TDevice : DeviceDefinition
        {
            private readonly TDevice _device;

            public PendingRemoval(TDevice device)
            {
                _device = device;
            }

            public override Task RemoveFromDeviceManager(DeviceManager deviceManager)
            {
                return deviceManager.RemoveDevice(_device);
            }

            public override bool TryGetDevice<T>([NotNullWhen(true)] out T? device)
                where T : class
            {
                if (typeof(T) != typeof(TDevice))
                {
                    device = default;
                    return false;
                }

                device = _device as T;
                return device != null;
            }
        }
    }
}