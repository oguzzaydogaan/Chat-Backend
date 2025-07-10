using Exceptions;
using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Entities;
using System;

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
            if (chat == null)
                throw new MessageException(MessageErrorType.ChatNotFound);
            if (!chat.Users.Any(u => u.Id == message.UserId))
                throw new MessageException(MessageErrorType.UserNotMemberOfChat);
            chat.LastUpdate = message.Time;
            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

        }

        public async Task<Message> DeleteMessageAsync(int messageId)
        {
            var message = await _context.Messages.Include(m => m.Chat).ThenInclude(c => c!.Users).FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null)
                throw new MessageException(MessageErrorType.MessageNotFound);
            message.IsDeleted = true;
            message.Chat!.LastUpdate = DateTime.UtcNow;
            _context.Update(message);
            await _context.SaveChangesAsync();
            return message;
        }
    }
}
