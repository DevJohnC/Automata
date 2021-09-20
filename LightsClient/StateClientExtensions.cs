using System.Threading.Tasks;
using Automata;
using Automata.Devices;
using LightsShared;

namespace LightsClient
{
    public static class StateClientExtensions
    {
        public static Task TurnOn(this GrpcStateClient client,
            TrackingDeviceHandle<LightSwitch, LightSwitchState> lightSwitch)
        {
            return client.ChangeState(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device),
                new SetPowerState(LightSwitchPowerState.On));
        }
        
        public static Task TurnOff(this GrpcStateClient client,
            TrackingDeviceHandle<LightSwitch, LightSwitchState> lightSwitch)
        {
            return client.ChangeState(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device),
                new SetPowerState(LightSwitchPowerState.Off));
        }
        
        public static Task SetPowerLevel(this GrpcStateClient client,
            TrackingDeviceHandle<LightSwitch, LightSwitchState> lightSwitch,
            double powerLevel)
        {
            return client.ChangeState(
                new ResourceDocument<LightSwitch>(lightSwitch.DeviceId, lightSwitch.Device),
                new SetPowerLevel(powerLevel));
        }
    }
}