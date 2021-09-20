using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automata.HostServer.Data.EntityFramework
{
    public class KeyedResourceIdEntity
    {
        public Guid ResourceId { get; set; }

        public string KindName { get; set; } = default!;

        public string ResourceKey { get; set; } = default!;

        public static void ConfigureModel(ModelBuilder modelBuilder)
        {
            Builder()
                .HasKey(q => q.ResourceId);
            Builder()
                .HasIndex(q => new
                {
                    q.ResourceKey,
                    q.KindName
                })
                .IsUnique(true);

            EntityTypeBuilder<KeyedResourceIdEntity> Builder()
            {
                return modelBuilder.Entity<KeyedResourceIdEntity>();
            }
        }
    }
}