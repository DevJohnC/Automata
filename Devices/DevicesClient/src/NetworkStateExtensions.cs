using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Client.Resources;
using Automata.Kinds;

namespace Automata.Devices
{
    public static class NetworkStateExtensions
    {
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
        
        private static async Task<Dictionary<IAutomataServer, List<ResourceDocument<DeviceController>>>> GetStateControllers(
            this AutomataNetwork network,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<IAutomataServer, List<ResourceDocument<DeviceController>>>();
            var tasks = new List<Task<(IAutomataServer Server, List<ResourceDocument<DeviceController>> Controllers)>>(network.Servers.Count);
            foreach (var server in network.Servers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (server.SupportsDevices())
                {
                    tasks.Add(GetStateControllers(server, cancellationToken));
                }
            }

            while (tasks.Count > 0)
            {
                var task = await Task.WhenAny(tasks);
                var taskResult = await task;
                tasks.Remove(task);
                result.Add(taskResult.Server, taskResult.Controllers);
            }

            return result;
        }

        private static async Task<(IAutomataServer, List<ResourceDocument<DeviceController>>)> GetStateControllers(
            IAutomataServer server,
            CancellationToken cancellationToken)
        {
            var resourcesClient = server.CreateService<IResourceClient>();
            var stateControllers = new List<ResourceDocument<DeviceController>>();
            await foreach (var stateController in resourcesClient
                .GetResources<DeviceController>()
                .WithCancellation(cancellationToken))
            {
                stateControllers.Add(stateController);
            }

            return (server, stateControllers);
        }
        
        public static async Task ChangeState<TDevice, TRequest>(
            this AutomataNetwork network,
            ResourceDocument<TDevice> device,
            TRequest request,
            CancellationToken cancellationToken = default)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            //  todo: refactor this to be readable
            //  todo: prefer the server hosting the device?
            var requestKind = KindModel.GetKind(typeof(TRequest));
            var serverControllerPair = (await network.GetStateControllers(cancellationToken))
                .SelectMany(q => q.Value.Select(
                    q2 => (Server: q.Key, Controller: q2)))
                .FirstOrDefault(q => 
                    SupportsMessageAndDevice(q.Controller.Record, device.ResourceId, requestKind));

            if (serverControllerPair == default)
                return;

            var client = serverControllerPair.Server.CreateService<IStateClient>();

            await client.RequestStateChange(
                serverControllerPair.Controller.ResourceId,
                device.ResourceId,
                new ResourceDocument<TRequest>(Guid.NewGuid(), request),
                cancellationToken);
        }
    }
}