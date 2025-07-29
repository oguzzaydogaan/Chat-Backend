namespace Services.DTOs
{
    public class ChatDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ChatWithNotSeensDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class CreateChatRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public List<int> UserIds { get; set; } = new List<int>();
    }

    public class CreateChatWithCreatorDTO
    {
        public CreateChatRequestDTO Chat { get; set; } = new();
        public UserDTO Creator { get; set; } = new();
    }

    public class CreateChatResponseDTO
    {
        public string Name { get; set; } = string.Empty;
        public List<UserDTO> Users { get; set; } = new List<UserDTO>();
    }

    public class SocketChatDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<UserDTO>? Users { get; set; }
    }

    public class ChatWithMessagesDTO
    {
        public string? Name { get; set; }
        public ICollection<MessageForChatDTO>? Messages { get; set; }
        public List<UserDTO>? Users { get; set; }
    }
}
