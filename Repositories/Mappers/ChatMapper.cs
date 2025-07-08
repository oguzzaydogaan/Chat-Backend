using Repositories.DTOs;
using Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
