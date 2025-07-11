namespace Exceptions
{
    public class ChatAlreadyExistException : Exception
    {
        public ChatAlreadyExistException()
            :base("Chat already exist")
        {
        }
    }
}
