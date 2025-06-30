using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DTOs
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }

    public class LoginRequestDTO
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponseDTO
    {
        public string? Token { get; set; }
        public int Id { get; set; }
        public DateTime ExpiresIn { get; set; }
    }
}
