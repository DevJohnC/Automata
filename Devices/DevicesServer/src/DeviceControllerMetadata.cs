using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Automata.Kinds;

namespace Automata.Devices
{
    public class DeviceControllerMetadata
    {
        private readonly List<SingularKindUri> _requestKinds;
        public IDeviceController Controller { get; }
        
        public List<DeviceControlMessageInvoker> MessageInvokers { get; }
        
        public Guid ControllerId { get; }

        public DeviceControllerMetadata(IDeviceController deviceController, Guid controllerId,
            List<DeviceControlMessageInvoker> messageInvokers)
        {
            ControllerId = controllerId;
            Controller = deviceController;
            MessageInvokers = messageInvokers;
            _requestKinds = messageInvokers.Select(q => q.RequestKind.Name.SingularUri).ToList();
        }

        public bool TryGetMessageInvoker(
            KindUri controlMessageKindUri,
            [NotNullWhen(true)] out DeviceControlMessageInvoker? messageInvoker)
        {
            messageInvoker = MessageInvokers.FirstOrDefault(
                q => q.RequestKind.Name.MatchesUri(controlMessageKindUri));
            return messageInvoker != null;
        }

        public async Task<ResourceDocument<DeviceController>> GetResourceDocument(DeviceManager deviceManager)
        {
            var devices = new List<Guid>();
            await foreach (var device in Controller.GetSupportedDevices(deviceManager))
            {
                devices.Add(device.ResourceId);
            }
            
            return new ResourceDocument<DeviceController>(ControllerId,
                new DeviceController(
                    _requestKinds, 
                    devices));
        }
    }
}