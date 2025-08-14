namespace Exceptions
{
    public class UserNotMemberOfChatException : UIException
    {
        public UserNotMemberOfChatException()
            :base("User not member of chat")
        {
        }
    }
}
