using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Automata.HostServer.Data.EntityFramework
{
    public class HostServerDbContext : DbContext
    {
        public DbSet<KeyedResourceIdEntity> KeyedResourceIds { get; set; } = default!;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            KeyedResourceIdEntity.ConfigureModel(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = "automata.host.db"
            };
            optionsBuilder
                .UseSqlite(connectionStringBuilder.ConnectionString);
        }
    }
}