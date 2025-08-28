namespace Services.DTOs
{
    public class ChatDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ChatWithUnseenCountAndUserCountDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public int UserCount { get; set; }
    }
    public class CreateChatRequestDTO
    {
        public string Name { get; set; } = string.Empty;
        public List<int> UserIds { get; set; } = [];
    }
    public class CreateChatResponseDTO
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<UserDTO> Users { get; set; } = [];
    }
    public class CreateChatWithCreatorDTO
    {
        public CreateChatRequestDTO Chat { get; set; } = new();
        public UserDTO Creator { get; set; } = new();
    }
    public class ChatWithUsersDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<UserDTO>? Users { get; set; }
    }
    public class ChatWithMessagesAndUsersDTO
    {
        public string? Name { get; set; }
        public ICollection<MessageWithSenderAndSeensDTO>? Messages { get; set; }
        public ICollection<UserDTO>? Users { get; set; }
    }
}
