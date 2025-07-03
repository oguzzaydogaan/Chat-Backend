using Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs
{
    public class SocketMessageDTO
    {
        public string? Type { get; set; }
        public RequestPayloadDTO? Payload { get; set; }
    }

    public class ResponseSocketMessageDTO {
        public string? Type { get; set; }
        public MessageForChatDTO? Payload { get; set; }
    }


}
