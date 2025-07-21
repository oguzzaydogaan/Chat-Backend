using Exceptions;
using Microsoft.EntityFrameworkCore;
using Repositories.Context;
using Repositories.Entities;

namespace Repositories.Repositories
{
    public class ChatRepository : BaseRepository<Chat>
    {
        public ChatRepository(RepositoryContext context)
            : base(context)
        {
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

        public async Task<Chat?> GetByUserIdsAsync(List<int> userIds)
        {
            var chat = await DbSet
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Users.Count == userIds.Count && c.Users.All(u => userIds.Contains(u.Id)));
            return chat;
        }

        public async Task<List<Chat>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await DbSet.ToListAsync();
            return await DbSet
                .Where(c => c.Name.Contains(searchTerm))
                .ToListAsync();
        }
        public async Task<Chat> GetChatWithMessagesAndUsersAsync(int chatId)
        {
            var chat = await DbSet
                .Include(c => c.Users)
                .Include(c => c.Messages)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                throw new Exception("Chat cannot found");

            chat.Messages = chat.Messages.OrderByDescending(m => m.Time).ToList();

            return chat;
        }
    }
}
