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

        public override async Task GetResources(KindUri request, IServerStreamWriter<ResourceRecord> responseStream, ServerCallContext context)
        {
            await foreach (var resource in _resourceApi.GetResources(request.NativeKindUri)
                .WithCancellation(context.CancellationToken))
            {
                await responseStream.WriteAsync(
                    ResourceRecord.FromNative(resource));
            }
        }
    }
}