namespace Exceptions
{
    public class ChatNotFoundException : Exception
    {
        public ChatNotFoundException()
            :base("Chat not found")
        {
        }
    }
}
