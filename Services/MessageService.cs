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
        private readonly MessageReadRepository _messageReadRepository;
        private readonly ChatRepository _chatRepository;
        public MessageService(MessageRepository messageRepository, MessageReadRepository messageReadRepository, ChatRepository chatRepository, IMapper mapper)
        : base(mapper, messageRepository)
        {
            _messageRepository = messageRepository;
            _chatRepository = chatRepository;
            _messageReadRepository = messageReadRepository;
        }

        public async Task<Message> AddAsync(Message message)
        {
            var chat = await _chatRepository.GetChatWithUsersAsync(message.ChatId);
            if (!chat.Users.Any(u => u.Id == message.UserId))
            {
                throw new UserNotMemberOfChatException();
            }

            await _messageRepository.AddAsync(message);

            var messageRead = _mapper.Map<MessageRead>(message);
            message.Seens = [messageRead];

            chat.LastUpdate = message.Time;
            await _chatRepository.SaveChangesAsync();

            return message;
        }
        public async Task<Message> SoftDeleteAsync(int messageId, int uid)
        {
            var message = await _messageRepository.GetMessageWithChatAsync(messageId);
            if (uid != message.UserId)
            {
                throw new Exception("You can delete only your own messages");
            }
            message.Content = "This message was deleted";
            message.IsDeleted = true;
            message.ImageString = "";
            message.Chat!.LastUpdate = DateTime.UtcNow;
            foreach (var seen in message.Seens)
            {
                if (seen.UserId != message.UserId)
                {
                    _messageReadRepository.Remove(seen);
                }
            }
            message = await _messageRepository.UpdateAsync(message);
            return message;
        }
    }
}
