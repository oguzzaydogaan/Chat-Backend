namespace Exceptions
{
    public class MessageException : Exception
    {
        public MessageException()
        {
        }

        public MessageException(MessageErrorType err)
            : base(err.ToString())
        {
        }
    }

    public enum MessageErrorType
    {
        UserNotMemberOfChat,
        ChatNotFound,
        MessageNotFound,
    }
}
