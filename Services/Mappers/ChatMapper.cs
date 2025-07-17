using Repositories.Entities;
using Services.DTOs;

namespace Services.Mappers
{
    public static class ChatMapper
    {
        public static SocketChatDTO EntityToChatDTO(this Chat chat)
        {
            return new SocketChatDTO
            {
                Id = chat.Id,
                Name = chat.Name,
                Users = chat.Users.Select(u => new UserDTO { Id = u.Id, Name = u.Name })?.ToList()
            };
        }
        public static Chat ToChat(this CreateChatRequestDTO createChatRequestDTO, List<User> users)
        {
            return new Chat
            {
                Users = users,
                Name = createChatRequestDTO.Name,
                LastUpdate = DateTime.UtcNow
            };
        }
        public static ChatDTO ToChatDTO(this Chat chat)
        {
            return new ChatDTO
            {
                Id = chat.Id,
                Name = chat.Name
            };
        }

        public static ChatWithMessagesDTO ToChatWithMessagesDTO(this Chat chat)
        {
            return new ChatWithMessagesDTO
            {
                Name = chat.Name,
                Messages = chat.Messages.Select(m => m.ToMessageForChatDTO()).ToList(),
            };
        }

        public static CreateChatResponseDTO ToCreateChatResponseDTO(this Chat chat)
        {
            return new CreateChatResponseDTO
            {
                Name = chat.Name,
                Users = chat.Users.Select(u => u.ToUserDTO()).ToList()
            };
        }
    }
}
