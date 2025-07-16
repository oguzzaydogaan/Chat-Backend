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
        public string? Name { get; set; }
        public DateTime ExpiresIn { get; set; }
    }

    public class RegisterRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
