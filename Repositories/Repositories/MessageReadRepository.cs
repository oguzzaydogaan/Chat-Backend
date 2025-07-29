using Microsoft.EntityFrameworkCore;
using Repositories.Context;
using Repositories.Entities;

namespace Repositories.Repositories
{
    public class MessageReadRepository : BaseRepository<MessageRead>
    {
        public MessageReadRepository(RepositoryContext context)
            : base(context)
        {
        }

        public async Task<List<MessageRead>> GetByMessageIdAsync(int messageId)
        {
            var messageReads = await DbSet.Where(mr => mr.MessageId == messageId).ToListAsync();
            return messageReads;
        }
    }
}
