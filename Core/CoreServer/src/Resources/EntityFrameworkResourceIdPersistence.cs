using System;
using System.Linq;
using System.Threading.Tasks;
using Automata.HostServer.Data.EntityFramework;
using Automata.Kinds;
using Microsoft.EntityFrameworkCore;

namespace Automata.HostServer.Resources
{
    public class EntityFrameworkResourceIdPersistence : IResourceIdPersistence
    {
        private readonly IDbContextFactory<HostServerDbContext> _dbContextFactory;

        public EntityFrameworkResourceIdPersistence(IDbContextFactory<HostServerDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public Guid GetOrCreateResourceId(KindName kindName, string resourceKey)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var kindNameStr = kindName
                .ToString(KindNameFormat.Singular)
                .ToLowerInvariant();
            
            var resourceIdEntity = dbContext
                .KeyedResourceIds
                .SingleOrDefault(q => q.KindName == kindNameStr && q.ResourceKey == resourceKey);

            if (resourceIdEntity != null)
                return resourceIdEntity.ResourceId;

            resourceIdEntity = new()
            {
                ResourceId = Guid.NewGuid(),
                KindName = kindNameStr,
                ResourceKey = resourceKey
            };
            dbContext.KeyedResourceIds.Add(resourceIdEntity);
            dbContext.SaveChanges();

            return resourceIdEntity.ResourceId;
        }

        public async Task<Guid> GetOrCreateResourceIdAsync(KindName kindName, string resourceKey)
        {
            await using var dbContext = _dbContextFactory.CreateDbContext();

            var kindNameStr = kindName
                .ToString(KindNameFormat.Singular)
                .ToLowerInvariant();
            
            var resourceIdEntity = await dbContext
                .KeyedResourceIds
                .Where(q => q.KindName == kindNameStr && q.ResourceKey == resourceKey)
                .SingleOrDefaultAsync();

            if (resourceIdEntity != null)
                return resourceIdEntity.ResourceId;

            resourceIdEntity = new()
            {
                ResourceId = Guid.NewGuid(),
                KindName = kindNameStr,
                ResourceKey = resourceKey
            };
            dbContext.KeyedResourceIds.Add(resourceIdEntity);
            await dbContext.SaveChangesAsync();

            return resourceIdEntity.ResourceId;
        }
    }
}