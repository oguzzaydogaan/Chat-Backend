namespace Repositories.Entities
{
    public class Chat : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public virtual List<User> Users { get; set; } = new();
        public virtual List<Message> Messages { get; set; } = new();
        public List<Call> Calls { get; set; } = new();
    }
}
