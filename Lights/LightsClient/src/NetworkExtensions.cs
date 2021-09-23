using System.Collections.Generic;
using Automata.Client;
using Automata.Devices;
using LightsShared;

namespace LightsClient
{
    public static class NetworkExtensions
    {
        public static IAsyncEnumerable<DeviceHandle<LightSwitch, LightSwitchState>> GetLights(this AutomataNetwork network)
        {
            return network.GetDevices<LightSwitch, LightSwitchState>();
        }
    }
}