using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Client.Networking;
using Automata.Devices.GrpcServices;
using Automata.GrpcServices;
using Grpc.Core;

namespace Automata.Devices
{
    public class GrpcStateClient : IStateClient
    {
        public static IStateClient Factory(GrpcAutomataServer server)
        {
            return new GrpcStateClient(
                server,
                server.ChannelFactory.CreateChannel(server));
        }

        private readonly DeviceServices.DeviceServicesClient _client;
        
        private readonly GrpcAutomataServer _server;
        
        public IAutomataServer Server => _server;

        public GrpcStateClient(GrpcAutomataServer server, ChannelBase channel)
        {
            _server = server;
            _client = new GrpcServices.DeviceServices.DeviceServicesClient(channel);
        }

        public async Task RequestStateChange(Guid controllerId,
            Guid deviceId,
            ResourceDocument deviceControlRequest,
            CancellationToken cancellationToken = default)
        {
            await _client.RequestStateChangeAsync(new()
            {
                ControllerId = controllerId.ToString(),
                DeviceId = deviceId.ToString(),
                ControlRequest = ResourceRecord.FromNative(
                    deviceControlRequest.Serialize())
            }, cancellationToken: cancellationToken);
        }
    }
}