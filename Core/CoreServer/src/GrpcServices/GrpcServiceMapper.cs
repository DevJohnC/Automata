using System;
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
        private readonly Action<IEndpointRouteBuilder> _mapServiceDelegate;

        public GrpcServiceMapper(Action<IEndpointRouteBuilder> mapServiceDelegate)
        {
            _mapServiceDelegate = mapServiceDelegate;
        }

        public override void MapService(IEndpointRouteBuilder endpoints)
        {
            _mapServiceDelegate(endpoints);
        }
    }
}