using System;
using System.Collections.Generic;
using System.Linq;
using Automata.Client;
using Automata.Devices.GrpcServices;
using Automata.Kinds;

namespace Automata.Devices
{
    public class ServerDeviceStateControllers
    {
        public ServerDeviceStateControllers(IAutomataServer server,
            List<ResourceDocument<DeviceController>> stateControllers)
        {
            Server = server;
            StateControllers = stateControllers;
        }

        public IAutomataServer Server { get; }
        
        public List<ResourceDocument<DeviceController>> StateControllers { get; }

        public List<ResourceDocument<DeviceController>> GetCandidates<TDevice, TRequest>(
            ResourceDocument<TDevice> device)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            var requestKind = KindModel.GetKind(typeof(TRequest));
            return StateControllers
                .Where(q => SupportsMessageAndDevice(q.Record, device.ResourceId, requestKind))
                .ToList();
        }
        
        private static bool SupportsMessageAndDevice(
            DeviceController controller,
            Guid deviceId,
            KindModel requestKind)
        {
            if (!controller.DeviceIds.Contains(deviceId))
                return false;

            return controller.ControlRequestKinds.Any(
                q => requestKind.Name.MatchesUri(q));
        }
    }
}