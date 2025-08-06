using Repositories.Entities;

namespace Services.DTOs
{
    public class MessageDTO
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Time { get; set; }
        public bool IsDeleted { get; set; }
        public int UserId { get; set; }
        public int ChatId { get; set; }
        public bool IsSystem { get; set; }
        public string ImageString { get; set; } = "";
    }

    public class CreateMessageRequestDTO
    {
        public string Content { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int ChatId { get; set; }
        public string ImageString { get; set; } = string.Empty;
    }

    public class MessageWithSenderAndSeensDTO
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public UserDTO? Sender { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSystem { get; set; }
        public ICollection<MessageRead>? Seens { get; set; }
        public int ChatId { get; set; }
        public string ImageString { get; set; } = "";
    }

    public class BytesWithUsersDTO
    {
        public byte[] Bytes { get; set; } = [];
        public ICollection<User> Users { get; set; } = [];
    }
}
