namespace Exceptions
{
    public class UserNotMemberOfChatException : Exception
    {
        public UserNotMemberOfChatException()
            :base("User not member of chat")
        {
        }
    }
}
