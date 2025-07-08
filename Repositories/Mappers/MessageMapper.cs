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
                Sender = message.User?.ToUserDTO(),
                IsDeleted = message.IsDeleted,
                ChatId = message.ChatId,
            };
        }

        public static RequestPayloadDTO ToPayloadDTO(this Message message)
        {
            return new RequestPayloadDTO
            {
                UserId = message.UserId,
                ChatId = message.ChatId,
                Content = message.Content,
                MessageId = message.Id,
            };
        }

        public static Message ToMessage(this RequestPayloadDTO payload)
        {
            return new Message
            {
                UserId = payload.UserId,
                ChatId = payload.ChatId,
                Content = payload.Content,
            };
        }
    }
}
