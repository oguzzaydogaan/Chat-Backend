using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Entities;

namespace Repositories.Repositories
{
    public class MessageRepository
    {
        public MessageRepository(RepositoryContext context)
        {
            _context = context;
        }
        private readonly RepositoryContext _context;

        public async Task<Message?> GetMessageByIdAsync(int messageId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            return message;
        }
        public async Task AddMessageAsync(Message message)
        {
            var chat = await _context.Chats.Include(c => c.Users).FirstOrDefaultAsync(c => c.Id == message.ChatId);
            if (chat != null && !chat.Users.Any(u => u.Id == message.UserId))
                throw new Exception("User is not a member of the chat.");
            try
            {
                await _context.Messages.AddAsync(message);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error adding message: " + ex.Message);
            }
        }

        public async Task<Message> DeleteMessageAsync(int messageId)
        {
            var message = await _context.Messages.Include(m=>m.Chat).ThenInclude(c=>c!.Users).FirstOrDefaultAsync(m=>m.Id==messageId);
            if (message == null)
                throw new Exception("Message not found.");
            message.IsDeleted = true;
            try
            {
                _context.Update(message);
                await _context.SaveChangesAsync();
                return message;
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Error deleting message: " + ex.Message);
            }
        }
    }
}
