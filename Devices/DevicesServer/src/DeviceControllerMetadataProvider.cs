using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Automata.HostServer.Resources;
using Automata.Kinds;

namespace Automata.Devices
{
    public class DeviceControllerMetadataProvider
    {
        private readonly IResourceIdPersistence _resourceIds;
        private readonly KindModel _controllerKind = KindModel.GetKind(typeof(DeviceController));

        public DeviceControllerMetadataProvider(IResourceIdPersistence resourceIds)
        {
            _resourceIds = resourceIds;
        }

        public async Task<DeviceControllerMetadata> GetMetadata(IDeviceController deviceController)
        {
            var messageInvokers = GetMessageInvokers(deviceController)
                .ToList();

            var id = await _resourceIds.GetOrCreateResourceIdAsync(
                _controllerKind.Name, deviceController.UniqueIdentifier);
            
            return new DeviceControllerMetadata(
                deviceController, id, messageInvokers);
        }

        private IEnumerable<DeviceControlMessageInvoker> GetMessageInvokers(IDeviceController deviceController)
        {
            var type = deviceController.GetType();
            foreach (var interfaceType in type.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IDeviceController<,>))
                {
                    yield return CreateMessageInvoker(deviceController, interfaceType);
                }
            }
        }

        private DeviceControlMessageInvoker CreateMessageInvoker(IDeviceController deviceController, Type interfaceType)
        {
            return GetType()
                .GetMethod(nameof(CreateMessageInvokerImpl), BindingFlags.Instance | BindingFlags.NonPublic)
                .MakeGenericMethod(interfaceType.GetGenericArguments())
                .Invoke(this, new object[] { deviceController }) as DeviceControlMessageInvoker;
        }
        
        private DeviceControlMessageInvoker CreateMessageInvokerImpl<TDevice, TRequest>(
            IDeviceController<TDevice, TRequest> deviceController)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            return new DeviceControlMessageInvoker<TDevice, TRequest>(deviceController);
        }
    }
}