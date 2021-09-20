using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.GrpcServices;
using Automata.Kinds;

namespace Automata.Devices
{
    public class GrpcStateClient
    {
        private readonly ConditionalWeakTable<GrpcAutomataServer, List<ResourceDocument<DeviceController>>>
            _serverStateControllers = new();

        internal GrpcStateClient(GrpcAutomataNetwork network)
        {
            Network = network;
        }

        public GrpcAutomataNetwork Network { get; }

        private bool SupportsMessageAndDevice(DeviceController controller,
            Guid deviceId,
            KindModel requestKind)
        {
            if (!controller.DeviceIds.Contains(deviceId))
                return false;

            return controller.ControlRequestKinds.Any(
                q => requestKind.Name.MatchesUri(q));
        }

        public async Task ChangeState<TDevice, TRequest>(
            ResourceDocument<TDevice> device, TRequest request)
            where TDevice : DeviceDefinition
            where TRequest : DeviceControlRequest
        {
            //  todo: refactor this to be readable
            //  todo: prefer the server hosting the device?
            var requestKind = KindModel.GetKind(typeof(TRequest));
            var serverControllerPair = _serverStateControllers
                .SelectMany(q => q.Value.Select(
                    q2 => (Server: q.Key, Controller: q2)))
                .FirstOrDefault(q => 
                        SupportsMessageAndDevice(q.Controller.Record, device.ResourceId, requestKind));

            if (serverControllerPair == default)
                return;

            var client = new GrpcServices.DeviceServices.DeviceServicesClient(
                serverControllerPair.Server.ChannelFactory.CreateChannel(serverControllerPair.Server));

            client.RequestStateChangeAsync(new()
            {
                ControllerId = serverControllerPair.Controller.ResourceId.ToString(),
                DeviceId = device.ResourceId.ToString(),
                ControlRequest = ResourceRecord.FromNative(
                    new ResourceDocument<TRequest>(Guid.NewGuid(), request).Serialize())
            });
        }

        public async Task RefreshStateControllers(CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>(Network.Servers.Count);
            foreach (var server in Network.Servers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (server.SupportsDevices())
                {
                    tasks.Add(RefreshStateControllers(server, cancellationToken));
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task RefreshStateControllers(GrpcAutomataServer server, CancellationToken cancellationToken)
        {
            var stateControllers = new List<ResourceDocument<DeviceController>>();
            await foreach (var stateController in server
                .GetResources<DeviceController>()
                .WithCancellation(cancellationToken))
            {
                stateControllers.Add(stateController);
            }
            _serverStateControllers.AddOrUpdate(server, stateControllers);
        }
    }
}