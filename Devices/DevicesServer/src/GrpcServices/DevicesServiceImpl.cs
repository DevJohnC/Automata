using System;
using System.Threading.Tasks;
using Automata.GrpcServices;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Automata.Devices.GrpcServices
{
    public class DevicesServiceImpl : DeviceServices.DeviceServicesBase
    {
        private readonly DeviceManager _deviceManager;
        private readonly DeviceControllerManager _deviceControllerManager;

        public DevicesServiceImpl(DeviceManager deviceManager,
            DeviceControllerManager deviceControllerManager)
        {
            _deviceManager = deviceManager;
            _deviceControllerManager = deviceControllerManager;
        }

        public override async Task<Empty> RequestStateChange(StateChangeRequest request, ServerCallContext context)
        {
            await _deviceControllerManager.RequestDeviceStateChange(
                Guid.Parse(request.ControllerId),
                Guid.Parse(request.DeviceId),
                request.ControlRequest.ToNative());
            return new Empty();
        }
    }
}