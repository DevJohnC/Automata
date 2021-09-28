using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Automata.Devices;
using Automata.HostServer.Infrastructure;
using Automata.HostServer.Resources;
using Automata.Kinds;

namespace Automata.Computers.Agent
{
    public class LocalComputerManifestProvider : IResourceProvider
    {
        private static readonly KindModel ManifestKind = KindModel.GetKind(typeof(ComputerManifest));
        
        private readonly IResourceIdPersistence _resourceIds;
        private readonly Guid _computerId;

        public LocalComputerManifestProvider(IResourceIdPersistence resourceIds)
        {
            _resourceIds = resourceIds;
            _computerId = resourceIds.GetOrCreateResourceId(ManifestKind.Name,
                "LocalComputerId");
        }

        public async IAsyncEnumerable<ResourceDocument> GetResources()
        {
            var resourceId = await _resourceIds.GetOrCreateResourceIdAsync(
                ManifestKind.Name, "LocalResourceId");
            yield return new ResourceDocument<ComputerManifest>(
                resourceId,
                GetLocalSystemManifest());

            var deviceController = new DeviceController(
                new()
                {
                    KindModel.GetKind(typeof(TurnOff)).Name.SingularUri
                }, new()
                {
                    _computerId
                }
            );
            
            yield return new ResourceDocument<DeviceController>(
                await _resourceIds.GetOrCreateResourceIdAsync(deviceController),
                deviceController
                );
        }

        public async IAsyncEnumerable<ResourceDocument> GetAssociatedResources(ResourceIdentifier resourceIdentifier)
        {
            yield break;
        }

        private ComputerManifest GetLocalSystemManifest()
        {
            return new ComputerManifest(
                _computerId,
                OperatingSystem.Windows,
                GetHostname(),
                GetStartupTime(),
                GetPhysicalAddresses());
        }

        private string GetHostname()
        {
            return Dns.GetHostName();
        }

        private DateTime GetStartupTime()
        {
            return DateTime.UtcNow - TimeSpan.FromMilliseconds(Environment.TickCount64);
        }

        private List<string> GetPhysicalAddresses()
        {
            var result = new List<string>();

            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (IncludeInResults(networkInterface))
                    result.Add(Convert.ToHexString(networkInterface
                            .GetPhysicalAddress()
                            .GetAddressBytes()));
            }
            
            return result;

            bool IncludeInResults(NetworkInterface networkInterface)
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    return false;
                
                var ipProps = networkInterface.GetIPProperties();
                if (ipProps.GatewayAddresses.Count == 0)
                    return false;

                foreach (var address in ipProps.UnicastAddresses)
                {   
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork &&
                        address.Address.AddressFamily != AddressFamily.InterNetworkV6)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    return true;
                }
                
                return false;
            }
        }
    }
}