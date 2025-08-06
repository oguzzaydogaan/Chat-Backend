using Microsoft.AspNetCore.Http;
using Repositories.Entities;

namespace Services.DTOs
{
    public class RequestPayloadDTO
    {
        public CreateMessageRequestDTO? Message { get; set; }
        public int? Id { get; set; }
        public ICollection<int>? Ids { get; set; }
        public CreateChatRequestDTO? Chat { get; set; }
    }

    public class ResponsePayloadDTO
    {
        public MessageWithSenderAndSeensDTO? Message { get; set; }
        public ChatWithUsersDTO? Chat { get; set; }
        public ICollection<MessageRead>? MessageReads { get; set; }
        public string? Error { get; set; }
    }
}
