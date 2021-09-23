using System.Threading.Tasks;
using Automata;
using Automata.Client;
using Automata.Devices;
using LightsShared;

namespace LightsClient
{
    public static class StateClientExtensions
    {
        public static Task TurnOn(this AutomataNetwork network,
            TrackingDeviceHandle<LightSwitch, LightSwitchState> lightSwitch)
        {
            return network.ChangeState(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device),
                new SetPowerState(LightSwitchPowerState.On));
        }
        
        public static Task TurnOff(this AutomataNetwork network,
            TrackingDeviceHandle<LightSwitch, LightSwitchState> lightSwitch)
        {
            return network.ChangeState(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device),
                new SetPowerState(LightSwitchPowerState.Off));
        }
        
        public static Task SetPowerLevel(this AutomataNetwork network,
            TrackingDeviceHandle<LightSwitch, LightSwitchState> lightSwitch,
            double powerLevel)
        {
            return network.ChangeState(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device),
                new SetPowerLevel(powerLevel));
        }
    }
}