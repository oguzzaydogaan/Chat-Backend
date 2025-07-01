using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs
{
    public class ChatDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class ChatWithMessagesDTO
    {
        public string? Name { get; set; }
        public ICollection<MessageForChatDTO>? Messages { get; set; }
    }
}
