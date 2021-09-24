using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rssdp;

namespace Automata.HostServer.Discoverability
{
    public class SsdpBackgroundService : BackgroundService
    {
        private ILogger<SsdpBackgroundService> _logger;
        private readonly IServer _server;
        private readonly SsdpRootDeviceAccessor _ssdpRootDeviceAccessor;
        private readonly IServerAddressesFeature _serverAddressFeature;

        public SsdpBackgroundService(ILogger<SsdpBackgroundService> logger, IServer server,
            SsdpRootDeviceAccessor ssdpRootDeviceAccessor)
        {
            _logger = logger;
            _server = server;
            _ssdpRootDeviceAccessor = ssdpRootDeviceAccessor;
            _serverAddressFeature = _server.Features.Get<IServerAddressesFeature>();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Waiting for server to start...");
            while (_serverAddressFeature.Addresses.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            
            var tasks = new List<Task>();
            foreach (var ipAddress in GetAllIpAddresses())
            {
                foreach (var hostAddress in _serverAddressFeature.Addresses)
                {
                    var hostUri = new Uri(hostAddress, UriKind.Absolute);
                    tasks.Add(RunOnIpAddress(
                        ipAddress, hostUri.Port, hostUri.Scheme, stoppingToken));
                }
            }

            await Task.WhenAll(tasks);
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

        private async Task RunOnIpAddress(string ipAddress, int hostingPort, string httpScheme, CancellationToken stoppingToken)
        {
            _logger.LogInformation($"Publishing SSDP on {ipAddress}");
            var deviceDefinition = await _ssdpRootDeviceAccessor.GetRootDevice(
                httpScheme, ipAddress, hostingPort);
            using var publisher = new SsdpDevicePublisher(new Rssdp.Infrastructure.SsdpCommunicationsServer(
                new SocketFactory(ipAddress)));
            publisher.AddDevice(deviceDefinition);

            try
            {
                await Task.Delay(-1, stoppingToken);
            }
            catch (TaskCanceledException e)
            {
            }
        }
    }
}