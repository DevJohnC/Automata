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
        public static async Task<NetworkDeviceStateControllers> GetStateControllers(
            this AutomataNetwork network,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<IAutomataServer, ServerDeviceStateControllers>();
            var tasks = new List<Task<ServerDeviceStateControllers>>(network.Servers.Count);
            foreach (var server in network.Servers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (server.SupportsDevices())
                {
                    tasks.Add(server.GetStateControllers(cancellationToken));
                }
            }

            while (tasks.Count > 0)
            {
                var task = await Task.WhenAny(tasks);
                var taskResult = await task;
                tasks.Remove(task);
                result.Add(taskResult.Server, taskResult);
            }

            return new NetworkDeviceStateControllers(
                result,
                network);
        }

        public static async Task<ServerDeviceStateControllers> GetStateControllers(
            this IAutomataServer server,
            CancellationToken cancellationToken = default)
        {
            var resourcesClient = server.CreateService<IResourceClient>();
            var controllerList = new List<ResourceDocument<DeviceController>>();
            await foreach (var stateController in resourcesClient
                .GetResources<DeviceController>()
                .WithCancellation(cancellationToken))
            {
                controllerList.Add(stateController);
            }

            return new ServerDeviceStateControllers(server, controllerList);
        }
        
        public static async Task ChangeState<TDevice, TRequest>(
            this AutomataNetwork network,
            ResourceDocument<TDevice> device,
            TRequest request,
            CancellationToken cancellationToken = default)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            var networkDeviceControllers = await network.GetStateControllers(cancellationToken);

            await networkDeviceControllers.ChangeState(device, request, cancellationToken);
        }

        public static async Task ChangeState<TDevice, TRequest>(
            this NetworkDeviceStateControllers networkDeviceControllers,
            ResourceDocument<TDevice> device,
            TRequest request,
            CancellationToken cancellationToken = default)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            foreach (var server in networkDeviceControllers.Network.Servers)
            {
                var serverDeviceControllers = networkDeviceControllers[server];
                if (serverDeviceControllers == null)
                    continue;
                var candidates = serverDeviceControllers.GetCandidates<TDevice, TRequest>(device);
                if (candidates.Count == 0)
                    continue;
                
                await server.ChangeState(
                    candidates[0],
                    device,
                    request,
                    cancellationToken);
                return;
            }

            throw new UnableToCompleteOperationException(
                "Couldn't find a device controller to handle change state request for device.",
                device, request);
        }
        
        public static async Task ChangeState<TDevice, TRequest>(
            this IAutomataServer server,
            ResourceDocument<TDevice> device,
            TRequest request,
            CancellationToken cancellationToken = default)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            var serverControllers = await server.GetStateControllers(cancellationToken);
            await serverControllers.ChangeState(device, request, cancellationToken);
        }

        public static async Task ChangeState<TDevice, TRequest>(
            this ServerDeviceStateControllers serverControllers,
            ResourceDocument<TDevice> device,
            TRequest request,
            CancellationToken cancellationToken = default)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            var controllerCandidates = serverControllers.GetCandidates<TDevice, TRequest>(
                device);

            if (controllerCandidates.Count == 0)
                throw new UnableToCompleteOperationException(
                    "Couldn't find a device controller to handle change state request for device.",
                    device, request);
            
            await serverControllers.Server.ChangeState(
                controllerCandidates[0],
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