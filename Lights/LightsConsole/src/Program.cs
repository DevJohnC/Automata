using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client.Networking;
using Automata.Client.Networking.Grpc;
using Automata.Client.Services;
using Automata.Devices;
using LightsShared;

namespace LightsConsole
{
    class Program
    {
        class MyAdHocNetwork : AdHocNetwork
        {
            private readonly IGrpcChannelFactory _channelFactory =
                new SharedChannelFactory(InsecureChannelFactory.SharedInstance);

            private readonly ServerServiceProvider<GrpcAutomataServer> _networkServices;

            public MyAdHocNetwork() :
                base(new SsdpServerLocator())
            {
                _networkServices = new ServerServiceProvider<GrpcAutomataServer>();
                _networkServices.AddDevices();
            }
            
            protected override Task<IAutomataServer?> CreateServer(IServerLocator locator, Uri uri, CancellationToken ct)
            {
                return Task.FromResult<IAutomataServer?>(new GrpcAutomataServer(
                    uri,
                    _networkServices,
                    _channelFactory));
            }
        }
        
        static async Task Main(string[] args)
        {
            using var network = new MyAdHocNetwork();

            var syncLock = new object();
            
            using var lightsMonitor = new DeviceMonitor<LightSwitch>(network);
            lightsMonitor.Changed += (_,_) =>
            {
                Render(syncLock, lightsMonitor.Devices);
            };

            Render(syncLock, lightsMonitor.Devices);

            while (true)
            {
                var nextKey = Console.ReadKey();
                if (nextKey.Key == ConsoleKey.Escape)
                    break;
                if (nextKey.Key == ConsoleKey.Enter)
                    await ChangeStates();
            }

            async Task ChangeStates()
            {
                /*var tasks = new List<Task>();
                foreach (var light in lightsWatcher.Devices)
                {
                    if (!light.TryGetState<LightSwitchState>(out var state))
                        continue;
                    
                    if (state.PowerState == LightSwitchPowerState.Off)
                        tasks.Add(network.TurnOn(light));
                    else
                        tasks.Add(network.TurnOff(light));
                }

                await Task.WhenAll(tasks);*/
            }
        }
        
        static void Render(object syncLock, IEnumerable<Device<LightSwitch>> lights)
        {
            lock (syncLock)
            {
                Console.Clear();
                Console.WriteLine("Lights");
                foreach (var light in lights)
                {
                    if (!light.TryGetState<LightSwitchState>(out var state))
                        continue;
                        
                    Console.WriteLine(
                        "  {0}: Is {1} at power level {2}",
                        light.Definition.NetworkId,
                        (state.PowerState == LightSwitchPowerState.On) ? "ON " : "OFF",
                        state.PowerLevel);
                }
                Console.WriteLine();
                    
                Console.WriteLine("Press [ENTER] to toggle state, [ESC] to exit");
            }
        }
    }
}