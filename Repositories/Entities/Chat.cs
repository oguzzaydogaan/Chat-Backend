namespace Repositories.Entities
{
    public class Chat : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public virtual List<User> Users { get; set; } = new List<User>();
        public virtual List<Message> Messages { get; set; } = new List<Message>();
    }
}
