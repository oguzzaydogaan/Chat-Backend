using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Entities
{
    public class Chat
    {
        public int Id { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
