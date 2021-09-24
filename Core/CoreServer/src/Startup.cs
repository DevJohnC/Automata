using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Automata.HostServer
{
    public class Startup
    {
        public virtual void ServicesConfiguring(IServiceCollection services)
        {
        }
        
        public virtual void ServicesConfigured(IServiceCollection services)
        {
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.ConfigureEndpointDefaults(endpointOpts =>
                {
                    endpointOpts.Protocols = HttpProtocols.Http2;
                });
            });
            
            ServicesConfiguring(services);

            services.TryAddSingleton<Discoverability.SsdpRootDeviceAccessor>();
            services.AddHostedService<Discoverability.SsdpBackgroundService>();
            services.TryAddTransient(typeof(IDbContextFactory<>), typeof(Data.EntityFramework.DefaultDbContextFactory<>));
            services.TryAddTransient<Resources.IResourceIdPersistence, Resources.EntityFrameworkResourceIdPersistence>();

            services.AddSingleton<Kinds.KindLibrary>();

            services.AddSingleton<Infrastructure.IResourceProvider>(sp =>
                sp.GetRequiredService<Kinds.KindLibrary>());

            services.TryAddTransient<Api.IResourceApi, Api.ResourceProviderResourceApi>();
            services.AddGrpc();
            
            ServicesConfigured(services);
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //  service retrieved instead of injected due to access level
            var grpcServiceMappers =
                app.ApplicationServices.GetRequiredService<IEnumerable<GrpcServices.GrpcServiceMapper>>();
            
            using (var dbContext = new Data.EntityFramework.HostServerDbContext())
            {
                dbContext.Database.EnsureCreated();
            }
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GrpcServices.ResourcesServiceImpl>();
                foreach (var serviceMapper in grpcServiceMappers)
                {
                    serviceMapper.MapService(endpoints);
                }
            });
        }
    }
}