using Exceptions;
using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Mappers;

namespace Repositories.Repositories
{
    public class ChatRepository : BaseRepository<Chat>
    {
        private readonly UserRepository _userRepository;
        public ChatRepository(RepositoryContext context, UserRepository userRepository)
            : base(context)
        {
            _userRepository = userRepository;
        }

        public async Task<Chat> AddUserToChat(int chatId, int userId)
        {
            var chat = await DbSet
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                throw new ChatNotFoundException();

            var user = await _userRepository
                .GetByIdAsync(userId);

            if (user == null)
                throw new UsersNotFoundException();

            if (chat.Users.Any(u => u.Id == userId))
                throw new UserAlreadyExistException();

            chat.Users.Add(user);
            chat.LastUpdate = DateTime.UtcNow;
            

            return await base.UpdateAsync(chat);
        }

        public async Task<Chat> GetChatWithUsersAsync(int chatId)
        {
            var chat = await DbSet
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);
            if (chat == null)
                throw new ChatNotFoundException();
            return chat;
        }

        public async Task<ChatWithMessagesDTO?> GetMessagesAsync(int chatId, int userId)
        {
            var chat = await DbSet
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                throw new Exception("Chat bulunamadı.");

            if (!chat.Users.Any(u => u.Id == userId))
                throw new Exception("Kullanıcı bu chat'e üye değil.");

            try
            {
                var messages = await _context.Set<Message>()
                .Where(m => m.ChatId == chatId && m.IsDeleted == false)
                .Include(m => m.User)
                .OrderBy(m => m.Time)
                .Select(m => m.ToMessageForChatDTO())
                .ToListAsync();

                string name = string.Join(string.Empty, chat!.Users
                    .Where(u => u.Id != userId)
                    .Select(u => u.Name + ", "));

                name = name.TrimEnd(',', ' ');

                return new ChatWithMessagesDTO
                {
                    Name = name,
                    Messages = messages
                };
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving chat messages.", ex);
            }
        }
    }
}
