using System;
using System.Collections.Generic;
using Automata.Kinds;

namespace Automata.Devices
{
    [Kind(Strings.KindGroup, Strings.CurrentKindVersion, "deviceController", "deviceControllers")]
    public sealed record DeviceController(
        List<SingularKindUri> ControlRequestKinds,
        List<Guid> DeviceIds) : Record;
}