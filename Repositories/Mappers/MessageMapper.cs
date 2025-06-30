using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.DTOs;
using Repositories.Entities;

namespace Repositories.Mappers
{
    public static class MessageMapper
    {
        public static MessageForChatDTO ToMessageForChatDTO(this Message message)
        {
            return new MessageForChatDTO
            {
                Id = message.Id,
                Content = message.Content,
                Time = message.Time,
                Sender = message.User?.ToUserDTO()
            };
        }
    }
}
