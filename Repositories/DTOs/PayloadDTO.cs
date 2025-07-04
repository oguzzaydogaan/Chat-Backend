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
        [JsonPropertyName("UserId")]
        public int UserID { get; set; }
        [JsonPropertyName("ChatId")]
        public int ChatID { get; set; }
        [JsonPropertyName("Content")]
        public string? Content { get; set; }
        [JsonPropertyName("MessageId")]
        public int? MessageID { get; set; }
    }
}
