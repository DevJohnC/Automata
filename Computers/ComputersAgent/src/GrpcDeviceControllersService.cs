using System.Threading.Tasks;
using Automata.Devices.GrpcServices;
using Automata.Kinds;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace Automata.Computers.Agent
{
    public class GrpcDeviceControllersService : DeviceServices.DeviceServicesBase
    {
        public override Task<Empty> RequestStateChange(StateChangeRequest request, ServerCallContext context)
        {
            var requestKindId = request.ControlRequest.KindUri.NativeKindUri;
            if (KindModel.GetKind(typeof(TurnOff)).Name.MatchesUri(requestKindId))
            {
                //  todo: check controller id and device id
                var turnOffRequest = request
                    .ControlRequest
                    .ToNative()
                    .Deserialize<TurnOff>();
            }
            return Task.FromResult(new Empty());
        }

        public override Task GetDeviceStatePairs(DeviceStatePairRequest request,
            IServerStreamWriter<DeviceStatePair> responseStream,
            ServerCallContext context)
        {
            return Task.CompletedTask;
        }
    }
}