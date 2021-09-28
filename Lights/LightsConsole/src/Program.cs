using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Client.Networking.Grpc;
using Automata.Client.Services;
using Automata.Devices;
using LightsClient;
using LightsShared;

namespace LightsConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var grpcNetworkServices = new ServerServiceProvider<GrpcAutomataServer>();
            grpcNetworkServices.AddDevices();

            var grpcChannelFactory = new SharedChannelFactory(InsecureChannelFactory.SharedInstance);
            
            var network = new AutomataNetwork();

            await using var ssdpServerLocator = new SsdpServerLocator();
            await using var networkWatcher = new NetworkResourceWatcher(network, ssdpServerLocator);
            
            var syncLock = new object();

            networkWatcher.ServerAvailable += async (sender, eventArgs, token) =>
            {
                await network.AddServer(new GrpcAutomataServer(
                    eventArgs.ServerUri,
                    grpcNetworkServices,
                    grpcChannelFactory), token);
            };

            var lightsWatcher = new DevicesWatcher<LightSwitch>(networkWatcher);
            lightsWatcher.Changed += StateChanged;

            await networkWatcher.StartAsync(default);
            
            Render();

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
                var tasks = new List<Task>();
                foreach (var light in lightsWatcher.Devices)
                {
                    if (!light.TryGetState<LightSwitchState>(out var state))
                        continue;
                    
                    if (state.PowerState == LightSwitchPowerState.Off)
                        tasks.Add(network.TurnOn(light));
                    else
                        tasks.Add(network.TurnOff(light));
                }

                await Task.WhenAll(tasks);
            }

            Task StateChanged(DevicesWatcher<LightSwitch> sender, CancellationToken cancellationToken)
            {
                Render();
                return Task.CompletedTask;
            }

            async void Render()
            {
                //lock (syncLock)
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Lights");
                    //foreach (var light in lightsWatcher.Devices)
                    await foreach (var light in network.GetLights())
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
                    
                    Console.WriteLine("Press [ENTER] to change state, [ESC] to exit");

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
            }
        }
    }
}