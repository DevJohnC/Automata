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
                    tasks.Add(GetServerControllers(server));
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

            async Task<(IAutomataServer Server, List<ResourceDocument<DeviceController>> Controllers)> GetServerControllers(
                IAutomataServer server)
            {
                var controllerList = new List<ResourceDocument<DeviceController>>();
                
                await foreach (var controller in server.GetStateControllers()
                    .WithCancellation(cancellationToken))
                {
                    controllerList.Add(controller);
                }

                return (server, controllerList);
            }
        }

        public static async IAsyncEnumerable<ResourceDocument<DeviceController>> GetStateControllers(
            this IAutomataServer server)
        {
            var resourcesClient = server.CreateService<IResourceClient>();
            await foreach (var stateController in resourcesClient
                .GetResources<DeviceController>())
            {
                yield return stateController;
            }
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
                throw new UnableToCompleteOperationException(
                    "Couldn't find a device controller to handle change state request for device.",
                    device, request);

            await serverControllerPair.Server.ChangeState(
                serverControllerPair.Controller,
                device,
                request,
                cancellationToken);
        }

        public static Task ChangeState<TDevice, TRequest>(
            this IAutomataServer server,
            ResourceDocument<DeviceController> controller,
            ResourceDocument<TDevice> device,
            TRequest request,
            CancellationToken cancellationToken = default)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            return server.CreateService<IStateClient>()
                .ChangeState(controller, device, request, cancellationToken);
        }
        
        public static async Task ChangeState<TDevice, TRequest>(
            this IStateClient client,
            ResourceDocument<DeviceController> controller,
            ResourceDocument<TDevice> device,
            TRequest request,
            CancellationToken cancellationToken = default)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            await client.RequestStateChange(
                controller.ResourceId,
                device.ResourceId,
                new ResourceDocument<TRequest>(Guid.NewGuid(), request),
                cancellationToken);
        }
    }
}