using AutoMapper;
using Exceptions;
using Repositories.Entities;
using Repositories.Repositories;
using Services.DTOs;

namespace Services
{
    public class MessageService : BaseService<Message, MessageDTO>
    {
        private readonly MessageRepository _messageRepository;
        private readonly ChatRepository _chatRepository;
        public MessageService(MessageRepository messageRepository, ChatRepository chatRepository, IMapper mapper)
        : base(mapper, messageRepository)
        {
            _messageRepository = messageRepository;
            _chatRepository = chatRepository;
        }
        

        public async Task<Message> AddAsync(Message message)
        {
            if (message == null)
                throw new ArgumentNullException("Message cannot be empty");

            var chat = await _chatRepository.GetChatWithUsersAsync(message.ChatId);
            if (!chat.Users.Any(u => u.Id == message.UserId))
                throw new UserNotMemberOfChatException();

            chat.LastUpdate = message.Time;
            await _chatRepository.UpdateAsync(chat);
            message = await _messageRepository.AddAsync(message);
            return message;
        }
        public async Task<Message> SoftDeleteAsync(int messageId)
        {
            var message = await _messageRepository.GetMessageWithChatAsync(messageId);
            message.Content = "This message was deleted";
            message.IsDeleted = true;
            message.Chat!.LastUpdate = DateTime.UtcNow;
            message = await _messageRepository.UpdateAsync(message);
            return message;
        }
    }
}
