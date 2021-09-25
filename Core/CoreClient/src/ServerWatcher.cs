using System;
using System.Threading;
using System.Threading.Tasks;
using Automata.Client.Resources;
using Automata.Kinds;
using Microsoft.Extensions.Hosting;

namespace Automata.Client
{
    public record ServerResourceEventArgs(
        IAutomataServer Server,
        SerializedResourceDocument SerializedResourceDocument);
    
    public class ServerWatcher : BackgroundService, IAsyncDisposable
    {
        public event AsyncEventHandler<ServerWatcher, ServerResourceEventArgs>? ResourceAvailable;
        
        public event AsyncEventHandler<ServerWatcher, ServerResourceEventArgs>? ResourceChanged;
        
        public event AsyncEventHandler<ServerWatcher, ServerResourceEventArgs>? ResourceUnavailable;
        
        public IAutomataServer Server { get; }

        public ServerWatcher(IAutomataServer server)
        {
            Server = server;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var resourceBaseKind = KindModel.GetKind(typeof(Record));
            var resourcesClient = Server.CreateService<IResourceClient>();
            await foreach (var resource in resourcesClient.GetResources(resourceBaseKind.Name)
                .WithCancellation(stoppingToken))
            {
                await (ResourceAvailable?.SerialInvoke(
                           this,
                           new(Server, resource),
                           stoppingToken)
                    ?? Task.CompletedTask);
            }
        }

        public async ValueTask DisposeAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await StopAsync(cts.Token);
        }
    }
}