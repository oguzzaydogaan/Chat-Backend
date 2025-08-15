namespace Exceptions
{
    public class ChatAlreadyExistException : UIException
    {
        public int RedirectChatId { get; set; }
        public ChatAlreadyExistException()
            : base("Chat already exist")
        {
        }

        public ChatAlreadyExistException(int id)
            : base($"Chat already exist with ID {id}")
        {
            RedirectChatId = id;
        }
    }
}


