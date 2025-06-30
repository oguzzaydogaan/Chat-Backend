using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public int ChatId { get; set; }
        public Chat? Chat { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
