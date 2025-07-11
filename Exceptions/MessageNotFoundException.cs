namespace Exceptions
{
    public class MessageNotFoundException : Exception
    {
        public MessageNotFoundException()
            :base("Message not found")
        {
        }
    }
}
