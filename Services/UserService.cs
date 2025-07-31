using AutoMapper;
using Exceptions;
using Microsoft.AspNetCore.Identity;
using Repositories.Entities;
using Repositories.Repositories;
using Services.DTOs;
using Services.Helpers.Mail_Helpers;
using System.Text.RegularExpressions;

namespace Services
{
    public class UserService : BaseService<User, UserDTO>
    {
        private readonly UserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly JwtService _jwtService;
        private readonly MailSender _mailSender;

        public UserService(UserRepository userRepository, PasswordHasher<User> passwordHasher, JwtService jwtService, IMapper mapper, MailSender mailSender)
        : base(mapper, userRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
            _mailSender = mailSender;
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
            user.EmailVerificationToken = Guid.NewGuid().ToString();
            await _userRepository.AddAsync(user);
            await _mailSender.SendEmailAsync(user.Email, user.EmailVerificationToken);
        }

        public async Task<bool> VerifyAsync(string email, string token)
        {
            var user = await _userRepository.GetByEmailAndTokenAsync(email, token);
            if (user == null)
            {
                throw new Exception("Invalid token or email");
            }
            if(user.IsEmailConfirmed == true)
            {
                return false;
            }
            user.IsEmailConfirmed = true;
            user.EmailVerificationToken = "";
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<LoginResponseDTO> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password!, password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new Exception("Invalid password");
            }

            if (user.IsEmailConfirmed == false)
            {
                throw new EmailVerificationException();
            }

            return _jwtService.Authenticate(user);
        }

        public async Task<List<ChatWithUnseenCountDTO>?> GetChatsAsync(int userId)
        {
            var user = await _userRepository.GetChatsAsync(userId);
            if (user.Chats == null)
                return null;
            var chats = user!.Chats.Select(c =>
            {
                if (c.Users.Count == 2)
                    c.Name = c.Users.FirstOrDefault(u => u.Id != userId)?.Name ?? throw new Exception("Other user not found");

                int count = !c.Messages[0].Seens.Any(s=>s.UserId == userId) ? -1 : c.Messages.Where(m => !m.Seens.Any(s => s.UserId == userId)).ToList().Count;

                return _mapper.Map<ChatWithUnseenCountDTO>(c, opt => opt.Items["Count"] = count);
            }).ToList();
            return chats;
        }

        public async Task<List<ChatDTO>> SearchChatsAsync(int userId, string searchTerm)
        {
            var chats = await _userRepository.SearchChatsAsync(userId, searchTerm);
            var dtos = chats.Select(c =>
            {
                if (c.Users.Count == 2)
                {
                    c.Name = c.Users.FirstOrDefault(u => u.Id != userId)?.Name ?? throw new Exception("Other user not found");
                }

                return _mapper.Map<ChatDTO>(c);
            }).ToList();
            return dtos;
        }
    }
}
