/*using System.Collections.Generic;

namespace Automata.Devices
{
    public record DeviceContextDelta<TDevice, TDeviceState>(
        IReadOnlyList<TDevice> RemovedDevices,
        IReadOnlyList<TDevice> AddedDevices,
        IReadOnlyDictionary<TDevice, TDeviceState> StateChanges)
        where TDevice : notnull, Device
        where TDeviceState : notnull, DeviceState;
    
    public interface IDeviceContextObserver
    {
        void OnDeviceAdded<TDevice, TDeviceState>(
            DeviceContext<TDevice, TDeviceState> deviceContext,
            TDevice device)
            where TDevice : notnull, Device
            where TDeviceState : notnull, DeviceState;

        void OnDeviceRemoved<TDevice, TDeviceState>(
            DeviceContext<TDevice, TDeviceState> deviceContext,
            TDevice device)
            where TDevice : notnull, Device
            where TDeviceState : notnull, DeviceState;

        void OnDeviceStateChange<TDevice, TDeviceState>(
            DeviceContext<TDevice, TDeviceState> deviceContext,
            TDevice device, TDeviceState state)
            where TDevice : notnull, Device
            where TDeviceState : notnull, DeviceState;
        
        void OnDeviceContextDelta<TDevice, TDeviceState>(
            DeviceContext<TDevice, TDeviceState> deviceContext,
            DeviceContextDelta<TDevice, TDeviceState> delta)
            where TDevice : notnull, Device
            where TDeviceState : notnull, DeviceState;
    }
}*/