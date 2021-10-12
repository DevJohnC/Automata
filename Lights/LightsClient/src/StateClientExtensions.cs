using System.Threading.Tasks;
using Automata;
using Automata.Client;
using Automata.Client.Networking;
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
            Device<LightSwitch> lightSwitch)
        {
            return network.TurnOn(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Definition));
        }
        
        public static Task TurnOff(this AutomataNetwork network,
            ResourceDocument<LightSwitch> lightSwitch)
        {
            return network.ChangeState(
                lightSwitch,
                new SetPowerState(LightSwitchPowerState.Off));
        }
        
        public static Task TurnOff(this AutomataNetwork network,
            Device<LightSwitch> lightSwitch)
        {
            return network.TurnOff(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Definition));
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
            Device<LightSwitch> lightSwitch,
            double powerLevel)
        {
            return network.SetPowerLevel(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Definition),
                powerLevel);
        }
    }
}