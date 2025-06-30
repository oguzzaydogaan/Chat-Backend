using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories.Repositories
{
    public class UserRepository
    {
        public UserRepository(RepositoryContext context, PasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }
        private readonly RepositoryContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new Exception("User not found.");
            return user;
        }

        public async Task<User?> AddUserAsync(User user)
        {
            try
            {
                user.Password = _passwordHasher.HashPassword(user, user.Password!);
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("An error occurred while adding the user.", ex);
            }
        }

        public async Task<User?> GetUsersChatsAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Chats)
                .ThenInclude(c => c.Users)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception("User not found.");
            return user;
        }

        public async Task LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new Exception("User not found.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password!, password);
            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Invalid password.");
        }
    }
}
