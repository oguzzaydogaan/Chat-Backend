using Exceptions;
using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Mappers;

namespace Repositories.Repositories
{
    public class ChatRepository : BaseRepository<Chat>
    {
        public ChatRepository(RepositoryContext context, UserRepository userRepository)
            : base(context)
        {
        }

        public async Task<Chat> AddUserToChat(int chatId, int userId)
        {
            var chat = await _context.Chats
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                throw new ChatNotFoundException();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new UsersNotFoundException();

            if (chat.Users.Any(u => u.Id == userId))
                throw new UserAlreadyExistException();

            chat.Users.Add(user);
            chat.LastUpdate = DateTime.UtcNow;
            _context.Chats.Update(chat);
            await _context.SaveChangesAsync();

            return chat;

        }

        public async Task<ChatWithMessagesDTO?> GetChatMessagesAsync(int chatId, int userId)
        {
            var chat = await _context.Chats
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                throw new Exception("Chat bulunamadı.");

            if (!chat.Users.Any(u => u.Id == userId))
                throw new Exception("Kullanıcı bu chat'e üye değil.");

            try
            {
                var messages = await _context.Messages
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

        public async Task DeleteChatAsync(int chatId)
        {
            var chat = await _context.Chats
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                throw new ChatNotFoundException();

            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();

            return;
        }
    }
}
