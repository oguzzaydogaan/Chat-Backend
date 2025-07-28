namespace Repositories.Entities
{
    public class MessageRead : BaseEntity
    {
        public int MessageId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public DateTime SeenAt { get; set; } = DateTime.UtcNow;
    }
}
