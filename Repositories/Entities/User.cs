﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        [EmailAddress]
        public string? Email { get; set; }
        public string? Password { get; set; }
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
