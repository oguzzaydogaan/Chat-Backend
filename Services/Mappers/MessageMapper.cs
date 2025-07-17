using Repositories.Entities;
using Services.DTOs;

namespace Services.Mappers
{
    public static class MessageMapper
    {
        public static GetAllMessagesResDTO ToGetAllMessagesResDTO(this Message message)
        {
            return new GetAllMessagesResDTO
            {
                Id = message.Id,
                Content = message.Content,
                Time = message.Time,
                IsDeleted = message.IsDeleted,
                UserId = message.UserId,
                ChatId = message.ChatId
            };
        }

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
