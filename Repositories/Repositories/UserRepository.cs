using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Mappers;

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

        public override async Task<User> AddAsync(User user)
        {
            var isTaken = await DbSet.FirstOrDefaultAsync(u => u.Email == user.Email) != null;
            if (!isTaken)
            {               
                return await base.AddAsync(user);
            }
            throw new Exception("This email already taken.");
        }
        public async Task<User?> GetUsersChatsAsync(int userId)
        {
            var user = await DbSet
                .Include(u => u.Chats)
                .ThenInclude(c => c.Users)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found.");
            user.Chats = user.Chats.OrderByDescending(c => c.LastUpdate).ToList();
            return user;
        }


    }
}
