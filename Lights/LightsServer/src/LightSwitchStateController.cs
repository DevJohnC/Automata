/*using System.Threading.Tasks;
using Automata.HostServer.Devices;
using Automata.HostServer.StateControllers;
using LightsShared;

namespace LightsServer
{
    public class LightSwitchStateController : DeviceStateController<LightSwitch, LightSwitchState,
        LightSwitchStateRecord>
    {
        private readonly ILightSwitchService _lightSwitchService;

        public LightSwitchStateController(
            IDeviceRetriever deviceRetriever,
            ILightSwitchService lightSwitchService) :
            base(deviceRetriever)
        {
            _lightSwitchService = lightSwitchService;
        }
        
        protected override void OnBuilding(
            DeviceStateControllerOptionsBuilder<LightSwitch, LightSwitchState, LightSwitchStateRecord> optionsBuilder)
        {
            optionsBuilder
                .SupportEnum(state => state.PowerState, ChangePowerState)
                .SupportAllValues();
            
            optionsBuilder
                .SupportScalar(state => state.PowerLevel, ChangePowerLevel)
                .Min(0)
                .Max(1);
        }

        private Task ChangePowerState(LightSwitch lightSwitch, LightSwitchPowerState newPowerState)
        {
            if (newPowerState == LightSwitchPowerState.Off)
            {
                return _lightSwitchService.TurnOff(lightSwitch);
            }

            return _lightSwitchService.TurnOn(lightSwitch);
        }

        private  Task ChangePowerLevel(LightSwitch lightSwitch, double newPowerLevel)
        {
            return _lightSwitchService.SetPowerLevel(lightSwitch, newPowerLevel);
        }
    }
}*/