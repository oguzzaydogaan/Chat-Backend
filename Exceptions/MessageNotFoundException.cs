namespace Exceptions
{
    public class MessageNotFoundException : UIException
    {
        public MessageNotFoundException()
            :base("Message not found")
        {
        }
    }
}
