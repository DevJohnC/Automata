using System.Collections.Generic;
using Automata.Client;

namespace Automata.Devices
{
    public class NetworkDeviceStateControllers
    {
        private readonly IReadOnlyDictionary<IAutomataServer, ServerDeviceStateControllers> _serverDeviceStateControllers;
        
        public NetworkDeviceStateControllers(
            IReadOnlyDictionary<IAutomataServer, ServerDeviceStateControllers> serverDeviceStateControllers,
            AutomataNetwork network)
        {
            _serverDeviceStateControllers = serverDeviceStateControllers;
            Network = network;
        }

        public AutomataNetwork Network { get; }

        public ServerDeviceStateControllers? this[IAutomataServer server]
        {
            get
            {
                _serverDeviceStateControllers.TryGetValue(server, out var result);
                return result;
            }
        }
    }
}