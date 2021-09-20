using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Automata.HostServer.GrpcServices
{
    internal abstract class GrpcServiceMapper
    {
        public abstract void MapService(IEndpointRouteBuilder endpoints);
    }
    
    internal class GrpcServiceMapper<TService> : GrpcServiceMapper
        where TService : class
    {
        public override void MapService(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGrpcService<TService>();
        }
    }
}