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

            await using var networkWatcher = new SsdpNetworkWatcher();

            networkWatcher.ServerAvailable += async (endpoint) =>
            {
                Console.WriteLine("Discovered server at: {0}", endpoint);
                var server = new GrpcAutomataServer(
                    endpoint,
                    grpcNetworkServices,
                    grpcChannelFactory);
                await network.AddServer(server, default);
            };
            
            await networkWatcher.Start(default);

            await Task.Delay(-1);

            if (network.Servers.Count == 0)
                throw new Exception("Couldn't connect to server.");

            var syncLock = new object();
            var lights = new List<TrackingDeviceHandle<LightSwitch, LightSwitchState>>();
            await foreach (var light in network.GetLights())
            {
                var trackingHandler = await light.GetTrackingHandle();
                trackingHandler.StateChanged += StateChanged;
                lights.Add(trackingHandler);
            }

            Render();

            while (true)
            {
                var nextKey = Console.ReadKey();
                if (nextKey.Key == ConsoleKey.Escape)
                    break;
                if (nextKey.Key == ConsoleKey.Enter)
                    await ChangeStates();
            }

            foreach (var light in lights)
            {
                await light.DisposeAsync();
            }

            async Task ChangeStates()
            {
                var tasks = new List<Task>();
                foreach (var light in lights)
                {
                    if (light.GetStateSnapshot().Record.PowerState == LightSwitchPowerState.Off)
                        tasks.Add(network.TurnOn(light));
                    else
                        tasks.Add(network.TurnOff(light));
                }

                await Task.WhenAll(tasks);
            }

            Task StateChanged(TrackingDeviceHandle sender, EventArgs eventArgs,
                CancellationToken cancellationToken)
            {
                Render();
                return Task.CompletedTask;
            }

            void Render()
            {
                lock (syncLock)
                {
                    Console.Clear();
                    Console.WriteLine("Lights");
                    foreach (var light in lights)
                    {
                        var state = light.GetStateSnapshot().Record;
                        Console.WriteLine(
                            "  {0}: Is {1} at power level {2}",
                            light.Device.NetworkId,
                            (state.PowerState == LightSwitchPowerState.On) ? "ON " : "OFF",
                            state.PowerLevel);
                    }
                    Console.WriteLine();
                    
                    Console.WriteLine("Press [ENTER] to change state, [ESC] to exit");
                }
            }
        }
    }
}