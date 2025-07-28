namespace Repositories.Entities
{
    public class Message : BaseEntity
    {
        public string Content { get; set; } = string.Empty;
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public int ChatId { get; set; }
        public Chat? Chat { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public bool IsDeleted { get; set; } = false;
        public bool IsSystem { get; set; } = false;
    }
}
