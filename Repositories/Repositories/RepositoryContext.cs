using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories.Repositories
{
    public class RepositoryContext : DbContext
    {
        public RepositoryContext(DbContextOptions<RepositoryContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Message> Messages { get; set; }
    }
}
