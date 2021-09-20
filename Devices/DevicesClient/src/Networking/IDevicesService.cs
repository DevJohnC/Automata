using System;
using System.Collections.Generic;
using System.Threading;
using Automata.Kinds;

namespace Automata.Devices.Networking
{
    public interface IDevicesService : IAsyncDisposable
    {
        IAsyncEnumerable<DeviceStatePair> GetDevicesFromServer(
            KindName deviceKind, KindName stateKind,
            CancellationToken cancellationToken = default);
    }
}