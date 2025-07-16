using Repositories.DTOs;
using Repositories.Entities;

namespace Repositories.Mappers
{
    public static class ChatMapper
    {
        public static SocketChatDTO EntityToChatDTO(this Chat chat)
        {
            return new SocketChatDTO
            {
                Id = chat.Id,
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
    }
}
