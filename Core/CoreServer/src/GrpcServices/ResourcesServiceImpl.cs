using System.Linq;
using System.Threading.Tasks;
using Automata.GrpcServices;
using Automata.HostServer.Api;
using Grpc.Core;

namespace Automata.HostServer.GrpcServices
{
    public class ResourcesServiceImpl : ResourcesService.ResourcesServiceBase
    {
        private readonly IResourceApi _resourceApi;

        public ResourcesServiceImpl(IResourceApi resourceApi)
        {
            _resourceApi = resourceApi;
        }

        public override async Task GetResources(ResourcesRequest request,
            IServerStreamWriter<ResourceGraph> responseStream,
            ServerCallContext context)
        {
            var associatedKinds = request.AssociatedKinds
                .Select(q => q.NativeKindUri)
                .ToList();
            
            await foreach (var resource in _resourceApi
                .GetResources(request.Kind.NativeKindUri)
                .WithCancellation(context.CancellationToken))
            {
                var graph = new ResourceGraph
                {
                    Resource = ResourceRecord.FromNative(resource)
                };
                if (associatedKinds.Count > 0)
                {
                    await foreach (var associatedResource in _resourceApi
                        .GetAssociatedResource(resource, associatedKinds)
                        .WithCancellation(context.CancellationToken))
                    {
                        graph.AssociatedRecords.Add(
                            ResourceRecord.FromNative(associatedResource));
                    }
                }

                await responseStream.WriteAsync(graph);
            }
        }
    }
}