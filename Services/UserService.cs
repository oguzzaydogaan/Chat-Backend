using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Repositories.Entities;
using Repositories.Repositories;
using Services.DTOs;
using System.Text.RegularExpressions;

namespace Services
{
    public class UserService : BaseService<User, UserDTO>
    {
        private readonly UserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtService _jwtService;

        public UserService(UserRepository userRepository, PasswordHasher<User> passwordHasher, JwtService jwtService, IMapper mapper)
        :base(mapper, userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task RegisterAsync(RegisterRequestDTO registerRequest)
        {
            Regex regex = new Regex("^(?=.*[A-Za-z])(?=.*[0-9])(?=.*[^a-zA-Z0-9]).{6,}$");
            if (regex.IsMatch(registerRequest.Password!) == false)
            {
                throw new Exception("Password must be at least 6 characters long and contain at least one letter, one number, and one special character");
            }
            var user = _mapper.Map<User>(registerRequest);
            var isTaken = await _userRepository.GetByEmailAsync(user.Email);
            if (isTaken != null)
            {
                throw new Exception("This email is already taken");
            }
            user.Password = _passwordHasher.HashPassword(user, user.Password!);
            await _userRepository.AddAsync(user);
        }

        public async Task<LoginResponseDTO> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password!, password);
            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Invalid password");

            return _jwtService.Authenticate(user);
        }

        public async Task<List<ChatDTO>?> GetChatsAsync(int userId)
        {
            var user = await _userRepository.GetChatsAsync(userId);
            if (user.Chats == null)
                return null;
            var chats = user!.Chats.Select(c =>
            {
                if (c.Users.Count == 2)
                    c.Name = c.Users.FirstOrDefault(u => u.Id != userId)?.Name ?? throw new Exception("Other user not found");

                return _mapper.Map<ChatDTO>(c);
            }).ToList();
            return chats;
        }     

        public async Task<List<ChatDTO>> SearchChatsAsync(int userId, string searchTerm)
        {
            var chats = await _userRepository.SearchChatsAsync(userId, searchTerm);
            var dtos = chats.Select(c =>
            {
                if (c.Users.Count == 2)
                    c.Name = c.Users.FirstOrDefault(u => u.Id != userId)?.Name ?? throw new Exception("Other user not found");

                return _mapper.Map<ChatDTO>(c);
            }).ToList();
            return dtos;
        }
    }
}
