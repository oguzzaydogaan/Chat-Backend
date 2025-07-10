namespace Exceptions
{
    public class ChatException : Exception
    {
        public ChatException()
        {
        }

        public ChatException(ChatErrorType err)
            : base(err.ToString())
        {
        }
    }

    public enum ChatErrorType
    {
        UsersNotFound,
        ChatNotFound,
        ChatAlreadyExist,
        UserAlreadyExist,
    }
}
