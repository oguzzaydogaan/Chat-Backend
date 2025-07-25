using Repositories.Entities;

namespace Services.DTOs
{
    public class MessageDTO
    {
    }

    public class CreateMessageRequestDTO
    {

    }

    public class GetAllMessagesResDTO
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Time { get; set; }
        public bool IsDeleted { get; set; }
        public int UserId { get; set; }
        public int ChatId { get; set; }
        public bool IsSystem { get; set; }
    }

    public class MessageForChatDTO
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Time { get; set; }
        public UserDTO? Sender { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSystem { get; set; }

        public int ChatId { get; set; }
    }

    public class MessageWithUsersDTO
    {
        public ICollection<User>? Users { get; set; }
        public Message Message { get; set; } = new Message();
    }

    public class BytesWithUsersDTO
    {
        public byte[]? Bytes { get; set; }
        public ICollection<User>? Users
        {
            get; set;
        }
    }
}
