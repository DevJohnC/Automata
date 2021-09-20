using System;
using System.Threading.Tasks;
using Automata.Kinds;

namespace Automata.Devices
{
    public abstract class DeviceControlMessageInvoker
    {
        protected DeviceControlMessageInvoker(KindModel requestKind)
        {
            RequestKind = requestKind;
        }

        public KindModel RequestKind { get; }
        
        public abstract Task Invoke(
            DeviceManager deviceManager,
            Guid deviceId,
            SerializedResourceDocument controlRequest);
    }

    public class DeviceControlMessageInvoker<TDevice, TControlRequest> : DeviceControlMessageInvoker
        where TDevice : DeviceDefinition
        where TControlRequest : DeviceControlRequest
    {
        private readonly IDeviceController<TDevice, TControlRequest> _deviceController;

        public DeviceControlMessageInvoker(IDeviceController<TDevice, TControlRequest> deviceController) :
            base(KindModel.GetKind(typeof(TControlRequest)))
        {
            _deviceController = deviceController;
        }

        public override async Task Invoke(
            DeviceManager deviceManager,
            Guid deviceId,
            SerializedResourceDocument controlRequest)
        {
            var deviceResult = await deviceManager.TryGetDevice<TDevice>(deviceId);
            if (!deviceResult.DidSucceed)
                //  todo: return device not found result
                return;
            //  todo: return a result from the controller
            await _deviceController.ChangeDeviceState(
                deviceResult.Result,
                controlRequest.Deserialize<TControlRequest>().Record);
        }
    }
}