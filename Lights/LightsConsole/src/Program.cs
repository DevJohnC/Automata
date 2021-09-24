using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Automata;
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
            
            var syncLock = new object();
            var lights = new List<TrackingDeviceHandle<LightSwitch, LightSwitchState>>();

            network.ServerAdded += async (_, server, ct) =>
            {
                await foreach (var light in server.GetLights()
                    .WithCancellation(ct))
                {
                    var trackingHandler = await light.GetTrackingHandle(ct);
                    trackingHandler.StateChanged += StateChanged;
                    lights.Add(trackingHandler);
                }
                Render();
            };
            
            Render();

            await network.AddNetworkWatcher(networkWatcher, endpoint =>
                new GrpcAutomataServer(endpoint, grpcNetworkServices, grpcChannelFactory));

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