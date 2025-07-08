using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.DTOs;
using Repositories.Entities;
using Repositories.Repositories;

namespace Services
{
    public class ChatService
    {
        public ChatService(ChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }
        private readonly ChatRepository _chatRepository;

        public async Task<Chat?> AddChatAsync(List<int> userIds)
        {
            var chat = await _chatRepository.AddChatAsync(userIds);
            return chat;
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
