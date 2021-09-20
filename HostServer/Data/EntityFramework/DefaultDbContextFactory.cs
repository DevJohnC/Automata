using Microsoft.EntityFrameworkCore;

namespace Automata.HostServer.Data.EntityFramework
{
    public class DefaultDbContextFactory<T> : IDbContextFactory<T>
        where T : DbContext, new()
    {
        public T CreateDbContext()
        {
            return new T();
        }
    }
}