using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Entities;

namespace Repositories.DTOs
{
    public class MessageDTO
    {
    }

    public class MessageForChatDTO
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Time { get; set; }
        public UserDTO? Sender { get; set; }
        public bool IsDeleted { get; set; }
        public int ChatId { get; set; }
    }
}
