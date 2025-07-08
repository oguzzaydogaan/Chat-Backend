using Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs
{
    public class SocketMessageDTO { }
    public class RequestSocketMessageDTO
    {
        public string? Type { get; set; }
        public RequestPayloadDTO Payload { get; set; } = new();
    }

    public class ResponseSocketMessageDTO {
        public string? Type { get; set; }
        public ResponsePayloadDTO Payload { get; set; } = new();
    }


}
