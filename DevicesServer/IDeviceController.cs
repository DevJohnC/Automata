using System.Collections.Generic;
using System.Threading.Tasks;

namespace Automata.Devices
{
    public interface IDeviceController
    {
        string UniqueIdentifier { get; }

        IAsyncEnumerable<ResourceDocument> GetSupportedDevices(DeviceManager deviceManager);
    }
    
    public interface IDeviceController<TDevice, TControlRequest> : IDeviceController
        where TDevice : DeviceDefinition
        where TControlRequest : DeviceControlRequest
    {
        Task ChangeDeviceState(TDevice device, TControlRequest request);
    }
}