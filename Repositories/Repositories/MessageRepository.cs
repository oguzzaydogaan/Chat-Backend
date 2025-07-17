using Exceptions;
using Microsoft.EntityFrameworkCore;
using Repositories.Context;
using Repositories.Entities;

namespace Repositories.Repositories
{
    public class MessageRepository : BaseRepository<Message>
    {
        public MessageRepository(RepositoryContext context)
            : base(context)
        {
        }

        public async Task<Message> GetMessageWithChatAsync(int messageId)
        {
            var message = await DbSet.Include(m => m.Chat).ThenInclude(c => c!.Users).FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null)
                throw new MessageNotFoundException();
            return message;
        }
    }
}
