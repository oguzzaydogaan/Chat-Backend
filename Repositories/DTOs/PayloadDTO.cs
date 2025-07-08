using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Repositories.DTOs
{
    public class RequestPayloadDTO
    {
        public int UserId { get; set; }
        public int ChatId { get; set; }
        public string? Content { get; set; }
        public int? MessageId { get; set; }
        public List<int>? UserIds { get; set; }
    }

    public class ResponsePayloadDTO
    {
        public MessageForChatDTO? Message { get; set; }
        public SocketChatDTO? Chat { get; set; }
    }
}
