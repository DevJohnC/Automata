using System.Collections.Generic;
using System.Threading.Tasks;
using Automata;
using Automata.Devices;
using LightsShared;

namespace LightsServer
{
    public class LightSwitchController :
        IDeviceController<LightSwitch, SetPowerLevel>,
        IDeviceController<LightSwitch, SetPowerState>
    {
        private readonly ILightSwitchService _lightSwitchService;

        public LightSwitchController(ILightSwitchService lightSwitchService)
        {
            _lightSwitchService = lightSwitchService;
        }

        public async Task ChangeDeviceState(LightSwitch lightSwitch, SetPowerLevel request)
        {
            await _lightSwitchService.SetPowerLevel(lightSwitch, request.PowerLevel);
            //  todo: return a status code that indicates success
        }

        public async Task ChangeDeviceState(LightSwitch lightSwitch, SetPowerState request)
        {
            switch (request.PowerState)
            {
                case LightSwitchPowerState.Off:
                    await _lightSwitchService.TurnOff(lightSwitch);
                    break;
                case LightSwitchPowerState.On:
                    await _lightSwitchService.TurnOn(lightSwitch);
                    break;
            }
            
            //  todo: return a status code that indicates success
        }
        
        public IAsyncEnumerable<ResourceDocument> GetSupportedDevices(DeviceManager deviceManager)
        {
            return deviceManager.GetDevices<LightSwitch>();
        }

        public string UniqueIdentifier => GetType().Name;
    }
}