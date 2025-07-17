using Exceptions;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Repositories;

namespace Services
{
    public class MessageService
    {
        public MessageService(MessageRepository messageRepository, ChatRepository chatRepository)
        {
            _messageRepository = messageRepository;
            _chatRepository = chatRepository;
        }
        private readonly MessageRepository _messageRepository;
        private readonly ChatRepository _chatRepository;

        public async Task<MessageWithUsersDTO?> AddAsync(Message message)
        {
            if (message == null)
                return null;

            var chat = await _chatRepository.GetChatWithUsersAsync(message.ChatId);
            if (!chat.Users.Any(u => u.Id == message.UserId))
                throw new UserNotMemberOfChatException();

            chat.LastUpdate = message.Time;
            await _chatRepository.UpdateAsync(chat);
            await _messageRepository.AddAsync(message);
            var mWithUsers = new MessageWithUsersDTO
            {
                Users = chat.Users,
                Message = message
            };
            return mWithUsers;
        }
        public async Task<MessageWithUsersDTO> DeleteAsync(int messageId)
        {
            var message = await _messageRepository.GetMessageWithChatAsync(messageId);
            message.Content = "This message was deleted";
            message.IsDeleted = true;
            message.Chat!.LastUpdate = DateTime.UtcNow;
            await _messageRepository.UpdateAsync(message);
            var mWithUsers = new MessageWithUsersDTO
            {
                Users = message.Chat!.Users,
                Message = message
            };
            return mWithUsers;
        }
    }
}
