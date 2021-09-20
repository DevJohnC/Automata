using System.Collections.Generic;
using Automata.Devices;
using LightsShared;

namespace LightsClient
{
    public static class DevicesClientExtensions
    {
        public static IAsyncEnumerable<DeviceHandle<LightSwitch, LightSwitchState>> GetLights(this DevicesClient client)
        {
            return client.GetDevices<LightSwitch, LightSwitchState>();
        }
    }
}