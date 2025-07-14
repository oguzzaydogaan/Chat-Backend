using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Mappers;
using Repositories.Repositories;
using System.Text.RegularExpressions;

namespace Services
{
    public class UserService
    {
        public UserService(UserRepository userRepository, PasswordHasher<User> passwordHasher, JwtService jwtService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }
        private readonly UserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtService _jwtService;

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
            if (user.Chats == null)
            {
                return null;
            }
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

        public async Task<LoginResponseDTO> LoginAsync(string email, string password)
        {
            var user = await _userRepository.LoginAsync(email);
            if (user == null)
                throw new Exception("User not found.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password!, password);
            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Invalid password.");

            return _jwtService.Authenticate(user);
        }

        public async Task RegisterAsync(RegisterRequestDTO registerRequest)
        {
            try
            {
                Regex regex = new Regex("^(?=.*?[A-Za-z])(?=.*?[0-9])(?=.*?[^a-zA-Z0-9_]).{6,}$");
                if (regex.IsMatch(registerRequest.Password!) == false)
                {
                    throw new Exception("Password must be at least 6 characters long and contain at least one letter, one number, and one special character.");
                }
                await _userRepository.RegisterAsync(registerRequest.RegisterRequestDTOToUser());
            }
            catch (DbUpdateException)
            {
                throw new Exception("Database update error.");
            }
            catch(Exception ex) {
                throw new Exception(ex.Message);
            }
        }
    }
}
