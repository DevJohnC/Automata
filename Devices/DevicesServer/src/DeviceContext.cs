/*using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Automata.Kinds;
using Newtonsoft.Json.Linq;

namespace Automata.Devices
{
    public abstract class DeviceContext
    {
        private readonly List<IDeviceContextObserver> _observers = new();
        
        public abstract KindModel DeviceKindModel { get; }
        
        public abstract KindModel DeviceStateKindModel { get; }
        
        public abstract Task Run(CancellationToken stoppingToken);

        protected void OnDeviceAdded<TDevice, TDeviceState>(
            DeviceContext<TDevice, TDeviceState> self,
            TDevice device)
            where TDevice : notnull, Device
            where TDeviceState : notnull, DeviceState
        {
            lock (_observers)
            {
                foreach (var observer in _observers)
                {
                    observer.OnDeviceAdded(self, device);
                }
            }
        }
        
        protected void OnDeviceRemoved<TDevice, TDeviceState>(
            DeviceContext<TDevice, TDeviceState> self,
            TDevice device)
            where TDevice : notnull, Device
            where TDeviceState : notnull, DeviceState
        {
            lock (_observers)
            {
                foreach (var observer in _observers)
                {
                    observer.OnDeviceRemoved(self, device);
                }
            }
        }
        
        protected void OnDeviceStateChanged<TDevice, TDeviceState>(
            DeviceContext<TDevice, TDeviceState> self,
            TDevice device, TDeviceState state)
            where TDevice : notnull, Device
            where TDeviceState : notnull, DeviceState
        {
            lock (_observers)
            {
                foreach (var observer in _observers)
                {
                    observer.OnDeviceStateChange(self, device, state);
                }
            }
        }
        
        protected void OnDeviceContextDelta<TDevice, TDeviceState>(
            DeviceContext<TDevice, TDeviceState> self,
            DeviceContextDelta<TDevice, TDeviceState> delta)
            where TDevice : notnull, Device
            where TDeviceState : notnull, DeviceState
        {
            lock (_observers)
            {
                foreach (var observer in _observers)
                {
                    observer.OnDeviceContextDelta(self, delta);
                }
            }
        }

        public IDisposable Subscribe(IDeviceContextObserver observer)
        {
            lock (_observers)
            {
                _observers.Add(observer);
                return new ObserverHandle(this, observer);
            }
        }

        private void Unsubscribe(IDeviceContextObserver observer)
        {
            lock (_observers)
            {
                _observers.Remove(observer);
            }
        }

        private class ObserverHandle : IDisposable
        {
            private readonly DeviceContext _deviceContext;
            private readonly IDeviceContextObserver _deviceContextObserver;

            public ObserverHandle(DeviceContext deviceContext,
                IDeviceContextObserver deviceContextObserver)
            {
                _deviceContext = deviceContext;
                _deviceContextObserver = deviceContextObserver;
            }
            
            public void Dispose()
            {
                _deviceContext.Unsubscribe(_deviceContextObserver);
            }
        }
    }

    public abstract class DeviceContext<TDevice, TDeviceState> : DeviceContext
        where TDevice : notnull, IServerDevice
        where TDeviceState : notnull, DeviceState
    {
        private record Delta(
            DeviceSet<TDevice> Devices,
            DeviceStates<TDevice, TDeviceState> States);

        private Delta? _oldDelta;
        
        public override KindModel DeviceKindModel { get; } = KindModel.GetKind(typeof(TDevice));

        public override KindModel DeviceStateKindModel { get; } = KindModel.GetKind(typeof(TDeviceState));

        public DeviceSet<TDevice> Devices { get; private set; }

        public DeviceStates<TDevice, TDeviceState> DeviceStates { get; private set; }

        public DeviceContext()
        {
            Devices = new();
            DeviceStates = new();
            
            Devices.DeviceAdded += DeviceAdded;
            Devices.DeviceRemoved += DeviceRemoved;
            DeviceStates.DeviceStateChanged += DeviceStateChanged;
        }

        private void DeviceStateChanged(object? sender, (TDevice Device, TDeviceState DeviceState) e)
        {
            if (_oldDelta == null)
                OnDeviceStateChanged(this, e.Device, e.DeviceState);
        }

        private void DeviceRemoved(object? sender, TDevice e)
        {
            if (_oldDelta == null)
                OnDeviceRemoved(this, e);
        }

        private void DeviceAdded(object? sender, TDevice e)
        {
            if (_oldDelta == null)
                OnDeviceAdded(this, e);
        }

        protected void BeginNewDelta()
        {
            if (_oldDelta != null)
            {
                throw new InvalidOperationException("Cannot begin a new delta when a new delta is already being constructed.");
            }

            _oldDelta = new Delta(Devices, DeviceStates);
            Devices = new DeviceSet<TDevice>();
            DeviceStates = new DeviceStates<TDevice, TDeviceState>();
            
            Devices.DeviceAdded += DeviceAdded;
            Devices.DeviceRemoved += DeviceRemoved;
            DeviceStates.DeviceStateChanged += DeviceStateChanged;
        }

        protected void CommitNewDelta()
        {
            if (_oldDelta == null)
            {
                throw new InvalidOperationException("Must have a pending delta to commit.");
            }

            var removedDevices = new List<TDevice>();
            var addedDevices = new List<TDevice>();
            var newStates = new Dictionary<TDevice, TDeviceState>();
            foreach (var device in _oldDelta.Devices)
            {
                if (Devices[device.GetResourceKey()] == null)
                {
                    //  device removed
                    removedDevices.Add(device);
                }
            }

            foreach (var device in Devices)
            {
                if (_oldDelta.Devices[device.GetResourceKey()] == null)
                {
                    //  device added
                    addedDevices.Add(device);
                }

                _oldDelta.States.TryGetState(device, out var oldState);
                if (!DeviceStates.TryGetState(device, out var newState))
                {
                    continue;
                }

                if (oldState == null && newState != null ||
                    !AreEqual(oldState!, newState!))
                {
                    newStates.Add(device, newState!);
                }
            }

            _oldDelta.Devices.DeviceAdded -= DeviceAdded;
            _oldDelta.Devices.DeviceRemoved -= DeviceRemoved;
            _oldDelta.States.DeviceStateChanged -= DeviceStateChanged;
            _oldDelta = null;
            OnDeviceContextDelta<TDevice, TDeviceState>(
                this,
                new(
                    removedDevices, addedDevices, newStates));
        }

        private bool AreEqual(TDeviceState a, TDeviceState b)
        {
            var jsonA = JObject
                .FromObject(a)
                .ToString();
            var jsonB = JObject
                .FromObject(b)
                .ToString();

            return string.Equals(jsonA, jsonB, StringComparison.InvariantCulture);
        }

        protected void RollbackNewDelta()
        {
            if (_oldDelta == null)
            {
                throw new InvalidOperationException("Must have a pending delta to rollback.");
            }
            
            Devices = _oldDelta.Devices;
            DeviceStates = _oldDelta.States;
            _oldDelta = null;
        }
    }

    public class DeviceSet<TDevice> : IEnumerable<TDevice>
        where TDevice : notnull, Device
    {
        private readonly List<TDevice> _devices = new();
        private readonly Dictionary<string, TDevice> _devicesByResourceKey = new();

        internal EventHandler<TDevice> DeviceAdded;
        internal EventHandler<TDevice> DeviceRemoved;

        public TDevice? this[string resourceKey]
        {
            get
            {
                _devicesByResourceKey.TryGetValue(resourceKey, out var device);
                return device;
            }
        }

        public void Add(TDevice device)
        {
            _devicesByResourceKey.Add(device.GetResourceKey(), device);
            _devices.Add(device);
            DeviceAdded?.Invoke(this, device);
        }

        public void Remove(string resourceId)
        {
            if (!_devicesByResourceKey.TryGetValue(resourceId, out var device))
                return;
            
            _devicesByResourceKey.Remove(resourceId);
            _devices.Remove(device);
            DeviceRemoved?.Invoke(this, device);
        }

        public void Remove(TDevice device)
        {
            _devicesByResourceKey.Remove(device.GetResourceKey());
            _devices.Remove(device);
            DeviceRemoved?.Invoke(this, device);
        }
        
        public IEnumerator<TDevice> GetEnumerator()
        {
            return _devices.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class DeviceStates<TDevice, TDeviceState>
        where TDevice : notnull, Device
        where TDeviceState : notnull, DeviceState
    {
        internal EventHandler<(TDevice Device, TDeviceState DeviceState)> DeviceStateChanged;
        
        private readonly Dictionary<string, TDeviceState> _states = new();
        
        public bool TryGetState(TDevice device, [NotNullWhen(true)] out TDeviceState? deviceState)
        {
            return _states.TryGetValue(device.GetResourceKey(), out deviceState);
        }
        
        public void SetState(TDevice device, TDeviceState deviceState)
        {
            _states[device.GetResourceKey()] = deviceState;
            DeviceStateChanged?.Invoke(this, (device, deviceState));
        }
    }
}*/