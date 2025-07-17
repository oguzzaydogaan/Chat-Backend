using Microsoft.EntityFrameworkCore;

namespace Repositories.Repositories
{
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
        {
        }
    }
}
