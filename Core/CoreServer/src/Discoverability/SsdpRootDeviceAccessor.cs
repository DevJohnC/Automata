using System;
using System.Threading.Tasks;
using Automata.HostServer.Resources;
using Automata.Kinds;
using Rssdp;

namespace Automata.HostServer.Discoverability
{
    public class SsdpRootDeviceAccessor
    {
        private readonly KindModel _resourceKindModel = KindModel.GetKind(typeof(Record));
        
        private readonly IResourceIdPersistence _resourceIdPersistence;

        public SsdpRootDeviceAccessor(IResourceIdPersistence resourceIdPersistence)
        {
            _resourceIdPersistence = resourceIdPersistence;
        }
        
        public async Task<SsdpRootDevice> GetRootDevice(string httpScheme, string host, int httpPort)
        {
            var id = await _resourceIdPersistence.GetOrCreateResourceIdAsync(
                _resourceKindModel.Name, "HostServer");
            
            return new SsdpRootDevice()
            {
                CacheLifetime = TimeSpan.FromMinutes(30), //How long SSDP clients can cache this info.
                Location = new Uri($"{httpScheme}://{host}:{httpPort}/ssdpRootDevice.xml"), // Must point to the URL that serves your devices UPnP description document. 
                DeviceTypeNamespace = "automata",
                DeviceType = "HostServer",
                FriendlyName = "Automata Host Server",
                Manufacturer = "FragLabs",
                ModelName = "HostServer",
                Uuid = id.ToString()
            };
            
        }
    }
}