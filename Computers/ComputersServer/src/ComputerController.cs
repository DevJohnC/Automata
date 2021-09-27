using System.Collections.Generic;
using System.Threading.Tasks;
using Automata.Devices;

namespace Automata.Computers.Server
{
    public class ComputerController : IDeviceController<Computer, WakeUp>
    {
        public Task ChangeDeviceState(Computer device, WakeUp request)
        {
            //  send a WOL packet to each physical address (by retrieving the associated state)
            throw new System.NotImplementedException();
        }

        public string UniqueIdentifier => GetType().Name;
        
        public IAsyncEnumerable<ResourceDocument> GetSupportedDevices(DeviceManager deviceManager)
        {
            return deviceManager.GetDevices<Computer>();
        }
    }
}