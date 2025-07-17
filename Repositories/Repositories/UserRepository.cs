using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories.Context;
using Repositories.Entities;

namespace Repositories.Repositories
{
    public class UserRepository : BaseRepository<User>
    {
        private readonly PasswordHasher<User> _passwordHasher;
        public UserRepository(RepositoryContext context, PasswordHasher<User> passwordHasher)
            : base(context)
        {
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            var user = await DbSet.FirstOrDefaultAsync(u => u.Email == email);
            return user;
        }
        public async Task<List<User>?> GetByListOfIdsAsync(List<int> ids)
        {
            var users = await DbSet.Where(u => ids.Contains(u.Id)).ToListAsync();
            return users;
        }
        public async Task<User> GetChatsAsync(int userId)
        {
            var user = await DbSet
                .Include(u => u.Chats)
                .ThenInclude(c => c.Users)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found");
            user.Chats = user.Chats.OrderByDescending(c => c.LastUpdate).ToList();
            return user;
        }
    }
}
