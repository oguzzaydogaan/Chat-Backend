using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Repositories.Entities;
using Repositories.Repositories;

namespace Services
{
    public class UserService
    {
        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }
        private readonly UserRepository _userRepository;

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<User?> AddUserAsync(User user)
        {
            return await _userRepository.AddUserAsync(user);
        }

        public async Task<Object?> GetUsersChatsAsync(int userId)
        {
            var user = await _userRepository.GetUsersChatsAsync(userId);
            var chats = user!.Chats.Select(c =>
            {
                string name = string.Join("", c.Users.Select(u => u.Id != userId ? u.Name + ", " : ""));
                name = name.TrimEnd(',', ' ');
                return new
                {
                    Id = c.Id,
                    Name = name,
                };
            });
            return chats;
        }

        public async Task LoginAsync(string email, string password)
        {
            await _userRepository.LoginAsync(email, password);

        }
    }
}
