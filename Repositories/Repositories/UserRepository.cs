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

        public async Task<List<User>> GetVerifiedsAsync()
        {
            var users = await DbSet.Where(u=>u.IsEmailConfirmed==true).ToListAsync();
            return users;
        }
        public async Task<User?> GetByEmailAsync(string email)
        {
            var user = await DbSet.FirstOrDefaultAsync(u => u.Email == email);
            return user;
        }
        public async Task<User?> GetByEmailAndTokenAsync(string email, string token)
        {
            var user = await DbSet.FirstOrDefaultAsync(u => u.Email == email && u.EmailVerificationToken == token);
            return user;
        }
        public async Task<List<User>> GetByListOfIdsAsync(List<int> ids)
        {
            var users = await DbSet.Where(u => ids.Contains(u.Id)).ToListAsync();
            return users;
        }
        public async Task<User> GetChatsAsync(int userId)
        {
            var user = await DbSet
                .Include(u => u.Chats)
                .ThenInclude(c => c.Users)
                .Include(u => u.Chats)
                .ThenInclude(c => c.Messages)
                .ThenInclude(m => m.Seens)
                .AsSplitQuery()
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found");
            user.Chats = user.Chats.OrderByDescending(c => c.LastUpdate).ToList();
            return user;
        }

        public async Task<List<Chat>> SearchChatsAsync(int userId, string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                var user = await GetChatsAsync(userId);
                return user.Chats.ToList();
            }

            var chats = await DbSet
                .Where(u => u.Id == userId)
                .SelectMany(u => u.Chats)
                .Include(c => c.Users)
                .Where((c) =>
                    c.Users.Count == 2
                        ? c.Users.First(u => u.Id != userId).Name.ToLower().Contains(searchTerm.ToLower())
                        : c.Name.ToLower().Contains(searchTerm.ToLower())
                )
                .ToListAsync();

            return chats;
        }
    }
}
