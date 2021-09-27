using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client;
using Automata.Client.Services;
using Automata.Devices;
using Automata.Kinds;
using Microsoft.Extensions.Hosting;

namespace Automata.Computers.Server
{
    public class ComputerMonitoringService : BackgroundService
    {
        private static readonly KindModel ManifestKind = KindModel.GetKind(typeof(ComputerManifest));
        
        private readonly AutomataNetwork _network;
        private readonly ServerServiceProvider<GrpcAutomataServer> _serverServices;
        private readonly DeviceManager _deviceManager;
        private readonly IServerLocator[] _serverDiscoverers;

        public ComputerMonitoringService(
            AutomataNetwork network,
            IEnumerable<IServerLocator> serverDiscoverers,
            ServerServiceProvider<GrpcAutomataServer> serverServices,
            DeviceManager deviceManager)
        {
            _network = network;
            _serverServices = serverServices;
            _deviceManager = deviceManager;
            _serverDiscoverers = serverDiscoverers.ToArray();
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var networkWatcher = new NetworkWatcher(_network, _serverDiscoverers);
            networkWatcher.ServerAvailable += NetworkWatcherOnServerAvailable;
            networkWatcher.ResourceAvailable += NetworkWatcherOnResourceAvailable;

            try
            {
                await networkWatcher.StartAsync(stoppingToken);

                await Task.Delay(-1, stoppingToken);
            }
            finally
            {
                networkWatcher.ServerAvailable -= NetworkWatcherOnServerAvailable;
            }
        }

        private async Task NetworkWatcherOnResourceAvailable(NetworkWatcher sender,
            ServerResourceEventArgs e,
            CancellationToken cancellationToken)
        {
            if (ManifestKind.Name.MatchesUri(e.SerializedResourceDocument.KindUri))
            {
                var computerManifestResource = e.SerializedResourceDocument
                    .Deserialize<ComputerManifest>();
                var computer = new Computer(
                    computerManifestResource.Record.OperatingSystem);
                var state = new ComputerState(
                    computerManifestResource.Record.Hostname,
                    computerManifestResource.Record.StartupTimeUtc,
                    AvailabilityState.Up,
                    computerManifestResource.Record.PhysicalAddresses);

                await _deviceManager.AddDevice(new ResourceDocument<Computer>(
                    computerManifestResource.Record.ComputerId,
                    computer), state, this);
            }
        }

        private async Task NetworkWatcherOnServerAvailable(NetworkWatcher sender,
            NetworkWatcher.ServerEventArgs e,
            CancellationToken cancellationToken)
        {
            await sender.Network.AddServer(
                new GrpcAutomataServer(e.ServerUri, _serverServices),
                cancellationToken);
        }
    }
}