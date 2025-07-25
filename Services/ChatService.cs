using AutoMapper;
using Exceptions;
using Repositories.Entities;
using Repositories.Repositories;
using Services.DTOs;

namespace Services
{
    public class ChatService
    {
        private readonly ChatRepository _chatRepository;
        private readonly UserRepository _userRepository;
        private readonly IMapper _mapper;
        public ChatService(ChatRepository chatRepository, UserRepository userRepository, IMapper mapper)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }


        public async Task<List<ChatDTO>> GetAllAsync()
        {
            var chats = await _chatRepository.GetAllAsync();
            var dtos = chats.Select(c => _mapper.Map<ChatDTO>(c)).ToList();
            return dtos;
        }

        public async Task<Chat> AddAsync(CreateChatRequestDTO? chat)
        {
            if (chat == null || chat.UserIds.Count < 2)
                throw new Exception("At least two users are required to create a chat");

            var users = await _userRepository.GetByListOfIdsAsync(chat.UserIds);
            if (users == null || users.Count != chat.UserIds.Count)
                throw new Exception("Some users not found");

            if (users.Count == 2)
            {
                var findChat = await _chatRepository.GetByUserIdsAsync(chat.UserIds);
                if (findChat != null)
                    throw new ChatAlreadyExistException(findChat.Id);
            }

            var entity = _mapper.Map<Chat>(chat, opt =>
            {
                opt.Items["Users"] = users;
            });
            if (entity.Name == string.Empty)
                throw new Exception("Chat needs a name");

            var created = await _chatRepository.AddAsync(entity);
            return created;
        }

        public async Task<ChatWithMessagesDTO> GetChatWithMessagesAsync(int chatId, int userId)
        {
            var chat = await _chatRepository.GetChatWithMessagesAndUsersAsync(chatId);
            chat.Messages = chat.Messages.OrderBy(m => m.Time).ToList();

            if (!chat.Users.Any(u => u.Id == userId))
                throw new Exception("User is not member of chat");

            if (chat.Users.Count == 2)
                chat.Name = chat.Users.FirstOrDefault(u => u.Id != userId)?.Name ?? throw new Exception("Other user not found");

            var dto = _mapper.Map<ChatWithMessagesDTO>(chat);
            return dto;
        }

        public async Task<Chat> AddUserAsync(int chatId, int userId)
        {
            var chat = await _chatRepository.GetChatWithUsersAsync(chatId);
            if (chat.Users.Count == 2)
                throw new Exception("Cannot add user to personal chat");
            if (chat.Users.Any(u => u.Id == userId))
                throw new UserAlreadyExistException();

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UsersNotFoundException();


            chat.Users.Add(user);
            chat.LastUpdate = DateTime.UtcNow;

            var updated = await _chatRepository.UpdateAsync(chat);
            return updated;
        }

        public async Task<Chat> DeleteAsync(int chatId)
        {
            var chat = await _chatRepository.DeleteAsync(chatId);
            return chat;
        }

        public async Task<List<ChatDTO>> SearchAsync(string searchTerm)
        {
            var chats = await _chatRepository.SearchAsync(searchTerm);
            return chats.Select(c => _mapper.Map<ChatDTO>(c)).ToList();
        }
    }
}