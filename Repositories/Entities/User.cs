namespace Repositories.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
