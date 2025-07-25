using AutoMapper;
using Exceptions;
using Repositories.Entities;
using Repositories.Repositories;
using Services.DTOs;

namespace Services
{
    public class MessageService
    {
        private readonly MessageRepository _messageRepository;
        private readonly ChatRepository _chatRepository;
        private readonly IMapper _mapper;
        public MessageService(MessageRepository messageRepository, ChatRepository chatRepository, IMapper mapper)
        {
            _messageRepository = messageRepository;
            _chatRepository = chatRepository;
            _mapper = mapper;
        }
        

        public async Task<List<GetAllMessagesResDTO>> GetAllAsync()
        {
            var messages = await _messageRepository.GetAllAsync();
            var dtos = messages.Select(m => _mapper.Map<GetAllMessagesResDTO>(m)).ToList();
            return dtos;
        }

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
