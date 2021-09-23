using System.Collections.Generic;
using Automata.Client;
using Automata.Kinds;

namespace Automata.Devices
{
    public interface IDevicesClient : IAutomataNetworkService
    {
        IAsyncEnumerable<DeviceStatePair> GetDeviceStatePairs(
            KindUri deviceKindUri, KindUri stateKindUri);
    }
}