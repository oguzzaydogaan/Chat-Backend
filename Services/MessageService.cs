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

        public async Task<MessageWithUsersDTO?> AddMessageAsync(Message message)
        {
            if (message == null)
                return null;
            await _messageRepository.AddMessageAsync(message);
            var mWithUsers = new MessageWithUsersDTO
            {
                Users = message.Chat!.Users,
                Message = message
            };
            return mWithUsers;
        }
        public async Task<MessageWithUsersDTO> DeleteMessageAsync(int messageId)
        {
            var message = await _messageRepository.DeleteMessageAsync(messageId);
            var mWithUsers = new MessageWithUsersDTO
            {
                Users = message.Chat!.Users,
                Message = message
            };
            return mWithUsers;
        }
    }
}
