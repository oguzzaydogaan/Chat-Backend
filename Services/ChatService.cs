using Exceptions;
using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Mappers;
using Repositories.Repositories;

namespace Services
{
    public class ChatService
    {
        public ChatService(ChatRepository chatRepository, UserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }
        private readonly ChatRepository _chatRepository;
        private readonly UserRepository _userRepository;

        public async Task<Chat?> AddChatAsync(CreateChatRequestDTO chat)
        {
            var users = await _userRepository.SearchList(u => chat.UserIds.Contains(u.Id));
            if (users.Count != chat.UserIds.Count)
            {
                throw new Exception("Some users not found.");
            }
            if (users.Count == 2)
            {
                var findChat = await _chatRepository
                    .Search(c => c.Users.Count == users.Count && c.Users.All(u => users.Contains(u)));

                if (findChat != null)
                {
                    throw new ChatAlreadyExistException();
                }
            }

            var created = await _chatRepository.AddAsync(chat.ToChat(users));
            return created;
        }

        public async Task<ChatWithMessagesDTO?> GetChatMessagesAsync(int chatId, int userId)
        {
            var chat = await _chatRepository.GetChatMessagesAsync(chatId, userId);
            if (chat == null)
                throw new Exception("Chat not found.");
            return chat;
        }

        public async Task<Chat> AddUserToChatAsync(int chatId, int userId)
        {
            return await _chatRepository.AddUserToChat(chatId, userId);
        }

        public async Task DeleteChatAsync(int chatId)
        {
            await _chatRepository.DeleteChatAsync(chatId);
        }
    }
}
