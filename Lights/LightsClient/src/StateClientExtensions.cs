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
            ResourceDocument<LightSwitch> lightSwitch)
        {
            return network.ChangeState(
                lightSwitch,
                new SetPowerState(LightSwitchPowerState.On));
        }
        
        public static Task TurnOn(this AutomataNetwork network,
            DeviceHandle<LightSwitch, LightSwitchState> lightSwitch)
        {
            return network.TurnOn(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device));
        }
        
        public static Task TurnOn(this AutomataNetwork network,
            TrackingDeviceHandle<LightSwitch, LightSwitchState> lightSwitch)
        {
            return network.TurnOn(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device));
        }
        
        public static Task TurnOff(this AutomataNetwork network,
            ResourceDocument<LightSwitch> lightSwitch)
        {
            return network.ChangeState(
                lightSwitch,
                new SetPowerState(LightSwitchPowerState.Off));
        }
        
        public static Task TurnOff(this AutomataNetwork network,
            DeviceHandle<LightSwitch, LightSwitchState> lightSwitch)
        {
            return network.TurnOff(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device));
        }
        
        public static Task TurnOff(this AutomataNetwork network,
            TrackingDeviceHandle<LightSwitch, LightSwitchState> lightSwitch)
        {
            return network.TurnOff(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device));
        }
        
        public static Task SetPowerLevel(this AutomataNetwork network,
            ResourceDocument<LightSwitch> lightSwitch,
            double powerLevel)
        {
            return network.ChangeState(
                lightSwitch,
                new SetPowerLevel(powerLevel));
        }
        
        public static Task SetPowerLevel(this AutomataNetwork network,
            DeviceHandle<LightSwitch, LightSwitchState> lightSwitch,
            double powerLevel)
        {
            return network.SetPowerLevel(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device),
                powerLevel);
        }
        
        public static Task SetPowerLevel(this AutomataNetwork network,
            TrackingDeviceHandle<LightSwitch, LightSwitchState> lightSwitch,
            double powerLevel)
        {
            return network.SetPowerLevel(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device),
                powerLevel);
        }
    }
}