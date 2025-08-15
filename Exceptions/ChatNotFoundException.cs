namespace Exceptions
{
    public class ChatNotFoundException : UIException
    {
        public ChatNotFoundException()
            :base("Chat not found")
        {
        }
    }
}
