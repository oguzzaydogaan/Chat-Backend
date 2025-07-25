namespace Exceptions
{
    public class ChatAlreadyExistException : Exception
    {
        public int RedirectChatId { get; set; }
        public ChatAlreadyExistException()
            : base("Chat already exist")
        {
        }

        public ChatAlreadyExistException(int id)
            : base("Chat already exist")
        {
            RedirectChatId = id;
        }
    }
}


