using System;
using Automata.Events;
using Automata.Kinds;

namespace Automata.Devices
{
    public static class Events
    {
        [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "deviceAddedEvent", "deviceAddedEvents")]
        public sealed record DeviceAddedEvent : EventRecord;
        
        [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "deviceRemovedEvent", "deviceRemovedEvents")]
        public sealed record DeviceRemovedEvent : EventRecord;

        [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "deviceStateChangedEvent", "deviceStateChangedEvents")]
        public sealed record DeviceStateChangedEvent(
            Guid DeviceId,
            Guid StateId,
            SingularKindUri DeviceKindUri,
            SingularKindUri StateKindUri,
            SerializedResourceDocument State) : EventRecord
        {
            public static DeviceStateChangedEvent Create<TDevice, TState>(
                ResourceDocument<TDevice> deviceDocument,
                ResourceDocument<TState> stateDocument)
                where TDevice : DeviceDefinition
                where TState : DeviceState
            {
                return new DeviceStateChangedEvent(
                    deviceDocument.ResourceId,
                    stateDocument.ResourceId,
                    deviceDocument.KindUri,
                    stateDocument.KindUri,
                    stateDocument.Serialize());
            }
        }
    }
}