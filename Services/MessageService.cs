using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Repositories;

namespace Services
{
    public class MessageService
    {
        public MessageService(MessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }
        private readonly MessageRepository _messageRepository;

        public async Task<MessageWithUsersDTO> AddMessageAsync(Message message)
        {
            if (message == null)
                throw new ArgumentNullException("Message cannot be null.");
            return await _messageRepository.AddMessageAsync(message);
        }
        public async Task<MessageWithUsersDTO> DeleteMessageAsync(int messageId)
        {
            return await _messageRepository.DeleteMessageAsync(messageId);
        }
    }
}
