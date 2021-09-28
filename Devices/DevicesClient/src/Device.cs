using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Automata.Kinds;

namespace Automata.Devices
{
    public class Device
    {
        private readonly IReadOnlyList<SerializedResourceDocument> _states;

        public Device(
            Guid deviceId,
            DeviceDefinition definition,
            IReadOnlyList<SerializedResourceDocument> states)
        {
            _states = states;
            DeviceId = deviceId;
            Definition = definition;
        }

        public Guid DeviceId { get; }
        
        public DeviceDefinition Definition { get; }

        public IReadOnlyList<DeviceState> GetStates(params KindUri[] kinds)
        {
            //  todo: implement
            throw new NotImplementedException();
        }

        public bool TryGetState(KindUri kind, [NotNullWhen(true)] out DeviceState? deviceState)
        {
            //  todo: implement
            throw new NotImplementedException();
        }

        public bool TryGetState<TState>([NotNullWhen(true)] out TState? deviceState)
            where TState : DeviceState
        {
            var stateKind = KindModel.GetKind(typeof(TState));
            var serializedResource = _states.FirstOrDefault(
                q => stateKind.Name.MatchesUri(q.KindUri));
            if (serializedResource == null)
            {
                deviceState = default;
                return false;
            }

            deviceState = serializedResource.Deserialize<TState>().Record;
            return true;
        }
    }

    public class Device<TDevice> : Device
        where TDevice : DeviceDefinition
    {
        public Device(Guid deviceId, TDevice definition,
            IReadOnlyList<SerializedResourceDocument> states) :
            base(deviceId, definition, states)
        {
            Definition = definition;
        }

        public new TDevice Definition { get; }
    }
}