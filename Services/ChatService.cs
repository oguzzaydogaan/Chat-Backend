using AutoMapper;
using Exceptions;
using Repositories.Entities;
using Repositories.Repositories;
using Services.DTOs;

namespace Services
{
    public class ChatService : BaseService<Chat, ChatDTO>
    {
        private readonly ChatRepository _chatRepository;
        private readonly UserRepository _userRepository;
        private readonly MessageRepository _messageRepository;


        public ChatService(ChatRepository chatRepository, UserRepository userRepository, MessageRepository messageRepository, IMapper mapper)
            : base(mapper, chatRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _messageRepository = messageRepository;

        }

        public async Task<Chat> AddAsync(CreateChatRequestDTO? chat, UserDTO creator)
        {
            if (chat == null || chat.UserIds.Count < 2)
            {
                throw new Exception("At least two users are required to create a chat");
            }

            var users = await _userRepository.GetByListOfIdsAsync(chat.UserIds);
            if (users == null || users.Count != chat.UserIds.Count)
            {
                throw new Exception("Some users not found");
            }
            if (users.Count == 2)
            {
                var findChat = await _chatRepository.GetByUserIdsAsync(chat.UserIds);
                if (findChat != null)
                {
                    throw new ChatAlreadyExistException(findChat.Id);
                }
            }

            var entity = _mapper.Map<Chat>(chat, opt =>
            {
                opt.Items["Users"] = users;
            });
            if (entity.Name == string.Empty)
            {
                throw new Exception("Chat needs a name");
            }

            var created = await _chatRepository.AddAsync(entity);
            var message = new Message
            {
                UserId = creator.Id,
                ChatId = created.Id,
                Content = $"{creator.Name} created this chat.",
                IsSystem = true
            };
            var added = await _messageRepository.AddAsync(message);
            return created;
        }

        public async Task<ChatWithMessagesAndUsersDTO> GetChatWithMessagesAsync(int chatId, int userId)
        {
            var chat = await _chatRepository.GetChatWithMessagesAndUsersAsync(chatId);
            chat.Messages = chat.Messages.OrderBy(m => m.Time).ToList();

            if (!chat.Users.Any(u => u.Id == userId))
            {
                throw new Exception("User is not member of chat");
            }

            if (chat.Users.Count == 2)
            {
                chat.Name = chat.Users.FirstOrDefault(u => u.Id != userId)?.Name ?? throw new Exception("Other user not found");
            }

            var dto = _mapper.Map<ChatWithMessagesAndUsersDTO>(chat);
            return dto;
        }

        public async Task<Chat> GetChatWithUsersAsync(int chatId)
        {
            var chat = await _chatRepository.GetChatWithUsersAsync(chatId);
            return chat;
        }

        public async Task<(Chat, Message)> AddUserAsync(int chatId, int userId, UserDTO sender)
        {
            var chat = await _chatRepository.GetChatWithUsersAsync(chatId);
            if (chat.Users.Count == 2)
            {
                throw new Exception("Cannot add user to personal chat");
            }
            if (chat.Users.Any(u => u.Id == userId))
            {
                throw new UserAlreadyExistException();
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new UsersNotFoundException();

            }

            chat.Users.Add(user);
            chat.LastUpdate = DateTime.UtcNow;
            await _chatRepository.UpdateAsync(chat);

            var message = new Message()
            {
                UserId = sender.Id,
                ChatId = chatId,
                Content = $"{sender.Name} added {user.Name}.",
                IsSystem = true,
                Time = chat.LastUpdate
            };
            await _messageRepository.AddAsync(message);

            var messageRead = _mapper.Map<MessageRead>(message);
            message.Seens = new List<MessageRead> { messageRead };
            await _messageRepository.UpdateAsync(message);

            return (chat, message);
        }

        public async Task<List<ChatDTO>> SearchAsync(string searchTerm)
        {
            var chats = await _chatRepository.SearchAsync(searchTerm);
            return chats.Select(c => _mapper.Map<ChatDTO>(c)).ToList();
        }

        public async Task<List<UserDTO>> SearchUsersAsync(int chatId, string searchTerm)
        {
            var users = await _chatRepository.SearchUsersAsync(chatId, searchTerm);

            var dtos = users.Select(u => _mapper.Map<UserDTO>(u)).ToList();
            return dtos;
        }
    }
}