using Exceptions;
using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Entities;
using System;

namespace Repositories.Repositories
{
    public class MessageRepository : BaseRepository<Message>
    {
        public MessageRepository(RepositoryContext context)
            : base(context)
        {
        }

        public async Task<Message> DeleteMessageAsync(int messageId)
        {
            var message = await _context.Messages.Include(m => m.Chat).ThenInclude(c => c!.Users).FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null)
                throw new MessageNotFoundException();
            message.IsDeleted = true;
            message.Chat!.LastUpdate = DateTime.UtcNow;
            _context.Update(message);
            await _context.SaveChangesAsync();
            return message;
        }
    }
}
