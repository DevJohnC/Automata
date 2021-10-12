using System.Collections.Generic;
using Automata.Client;
using Automata.Client.Networking;
using Automata.Devices;
using LightsShared;

namespace LightsClient
{
    public static class NetworkExtensions
    {
        public static IAsyncEnumerable<Device<LightSwitch>> GetLights(
            this AutomataNetwork network)
        {
            return network.GetDevices<LightSwitch>();
        }

        public static IAsyncEnumerable<Device<LightSwitch>> GetLights(
            this IAutomataServer server)
        {
            return server.GetDevices<LightSwitch>();
        }
    }
}