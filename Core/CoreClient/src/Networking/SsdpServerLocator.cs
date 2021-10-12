using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Rssdp;

namespace Automata.Client.Networking
{
    public class SsdpServerLocator : BackgroundService, IServerLocator
    {
        private const string AutomataServerUrn = "urn:automata:device:HostServer:1";

        public event AsyncEventHandler<IServerLocator, Uri>? ServerAvailable;
        public event AsyncEventHandler<IServerLocator, Uri>? ServerUnavailable;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var tasks = new List<Task>();
            foreach (var ipAddress in GetAllIpAddresses())
            {
                tasks.Add(RunOnIpAddress(ipAddress, stoppingToken));
            }

            return Task.WhenAll(tasks);
        }

        private async Task RunOnIpAddress(string ipAddress, CancellationToken stoppingToken)
        {
            using var deviceLocator = new SsdpDeviceLocator(new Rssdp.Infrastructure.SsdpCommunicationsServer(
                new SocketFactory(ipAddress)));
            deviceLocator.NotificationFilter = AutomataServerUrn;
            deviceLocator.DeviceAvailable += async (s, e) =>
            {
                if (!e.IsNewlyDiscovered)
                    return;
                var task = ServerAvailable?.SerialInvoke(this,
                    new(
                        $"{e.DiscoveredDevice.DescriptionLocation.Scheme}://{e.DiscoveredDevice.DescriptionLocation.Host}:{e.DiscoveredDevice.DescriptionLocation.Port}"),
                    stoppingToken) ?? Task.CompletedTask;
                await task;
            };
            deviceLocator.DeviceUnavailable += async (s, e) =>
            {
                var task = ServerUnavailable?.SerialInvoke(this,
                    new($"{e.DiscoveredDevice.DescriptionLocation.Scheme}://{e.DiscoveredDevice.DescriptionLocation.Host}:{e.DiscoveredDevice.DescriptionLocation.Port}"),
                    stoppingToken) ?? Task.CompletedTask;
                await task;
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
            await StopAsync(cts.Token);
        }
    }
}