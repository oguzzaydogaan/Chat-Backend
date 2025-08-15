namespace Repositories.Entities
{
    public class Message : BaseEntity
    {
        public string LocalId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Time { get; set; } = DateTime.UtcNow;
        public int ChatId { get; set; }
        public Chat? Chat { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public List<MessageRead> Seens { get; set; } = [];
        public bool IsDeleted { get; set; } = false;
        public bool IsSystem { get; set; } = false;
        public string ImageString { get; set; } = string.Empty;
    }
}
