using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Rssdp;

namespace Automata.Client
{
    public class SsdpNetworkWatcher : INetworkWatcher
    {
        private const string AutomataServerUrn = "urn:automata:device:HostServer:1";
        
        private CancellationTokenSource? _stoppingCts;
        private Task? _executeTask;

        public event ServerDiscoveryEvent? ServerAvailable;
        public event ServerDiscoveryEvent? ServerUnavailable;

        public Task Start(CancellationToken cancellationToken)
        {
            _stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Store the task we're executing
            _executeTask = RunLoop(_stoppingCts.Token);

            // If the task is completed then return it, this will bubble cancellation and failure to the caller
            if (_executeTask.IsCompleted)
            {
                return _executeTask;
            }

            // Otherwise it's running
            return Task.CompletedTask;
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            if (_executeTask == null)
            {
                return;
            }

            try
            {
                _stoppingCts?.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executeTask, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
            }
        }

        private async Task RunLoop(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>();
            foreach (var ipAddress in GetAllIpAddresses())
            {
                tasks.Add(RunOnIpAddress(ipAddress, stoppingToken));
            }

            await Task.WhenAll(tasks);
        }

        private async Task RunOnIpAddress(string ipAddress, CancellationToken stoppingToken)
        {
            using var deviceLocator = new SsdpDeviceLocator(new Rssdp.Infrastructure.SsdpCommunicationsServer(
                new SocketFactory(ipAddress)));
            deviceLocator.NotificationFilter = AutomataServerUrn;
            deviceLocator.DeviceAvailable += (s, e) =>
            {
                if (!e.IsNewlyDiscovered)
                    return;
                ServerAvailable?.Invoke(
                    new($"{e.DiscoveredDevice.DescriptionLocation.Scheme}://{e.DiscoveredDevice.DescriptionLocation.Host}:{e.DiscoveredDevice.DescriptionLocation.Port}"));
            };
            deviceLocator.DeviceUnavailable += (s, e) =>
            {
                ServerUnavailable?.Invoke(
                    new($"{e.DiscoveredDevice.DescriptionLocation.Scheme}://{e.DiscoveredDevice.DescriptionLocation.Host}:{e.DiscoveredDevice.DescriptionLocation.Port}"));
            };
            deviceLocator.StartListeningForNotifications();
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await deviceLocator.SearchAsync(AutomataServerUrn, TimeSpan.FromSeconds(4));
                }
            }
            catch (TaskCanceledException e)
            {
            }
            finally
            {
                deviceLocator.StopListeningForNotifications();
            }
        }
        
        private IEnumerable<string> GetAllIpAddresses()
        {
            foreach (var networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;

                var ipProps = networkInterface.GetIPProperties();
                if (ipProps.GatewayAddresses.Count == 0)
                    continue;

                foreach (var address in ipProps.UnicastAddresses)
                {   
                    if (address.Address.AddressFamily != AddressFamily.InterNetwork &&
                        address.Address.AddressFamily != AddressFamily.InterNetworkV6)
                        continue;

                    if (IPAddress.IsLoopback(address.Address))
                        continue;

                    yield return address.Address.ToString();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await Stop(cts.Token);
        }
    }
}