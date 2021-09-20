using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Automata.Devices
{
    public class DeviceDelta
    {
        private readonly DeviceManager _deviceManager;

        private readonly object _ownerReference;
        private readonly List<PendingRemoval> _pendingRemovals;
        private readonly List<PendingDeviceAdd> _pendingDeviceAdds = new();

        internal DeviceDelta(DeviceManager deviceManager, object ownerReference,
            List<PendingRemoval> removals)
        {
            _deviceManager = deviceManager;
            _ownerReference = ownerReference;
            _pendingRemovals = removals;
        }
        
        public void AddDevice<TDevice, TState>(TDevice device, TState state)
            where TDevice : DeviceDefinition
            where TState : DeviceState
        {
            var pendingAdd = new PendingDeviceAdd<TDevice, TState>(
                device,
                state);
            _pendingDeviceAdds.Add(pendingAdd);

            var existingRemoval = _pendingRemovals
                .FindIndex(q => pendingAdd.MatchesRemoval(q));
            if (existingRemoval > -1)
                _pendingRemovals.RemoveAt(existingRemoval);
        }

        public async Task Apply()
        {
            //  build a list of tasks to wait to complete
            //  the DeviceManager will send out events for each add/remove/update
            //  but we don't want that to bottleneck this operation
            var tasks = new List<Task>();
            
            foreach (var pendingAdd in _pendingDeviceAdds)
            {
                tasks.Add(pendingAdd.AddToDeviceManager(_ownerReference, _deviceManager));
            }

            foreach (var pendingRemoval in _pendingRemovals)
            {
                tasks.Add(pendingRemoval.RemoveFromDeviceManager(_deviceManager));
            }

            await Task.WhenAll(tasks);
        }

        private abstract class PendingDeviceAdd
        {
            public abstract Task AddToDeviceManager(object owner, DeviceManager deviceManager);

            public abstract bool MatchesRemoval(PendingRemoval removal);
        }

        private class PendingDeviceAdd<TDevice, TState> : PendingDeviceAdd
            where TDevice : DeviceDefinition
            where TState : DeviceState
        {
            private readonly TDevice _device;
            private readonly TState _state;

            public PendingDeviceAdd(TDevice device, TState state)
            {
                _device = device;
                _state = state;
            }

            public override async Task AddToDeviceManager(object owner, DeviceManager deviceManager)
            {
                if (!await deviceManager.TryAddDevice(_device, _state, owner))
                    await deviceManager.UpdateDeviceState(_device, _state);
            }

            public override bool MatchesRemoval(PendingRemoval removal)
            {
                if (!removal.TryGetDevice<TDevice>(out var compareDevice))
                    return false;

                return _device.Equals(compareDevice);
            }
        }
        
        internal abstract class PendingRemoval
        {
            public abstract Task RemoveFromDeviceManager(DeviceManager deviceManager);

            public abstract bool TryGetDevice<T>([NotNullWhen(true)] out T? device)
                where T : DeviceDefinition;
        }
    }
}