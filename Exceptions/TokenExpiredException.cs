namespace Exceptions
{
    public class TokenExpiredException : Exception
    {
        public TokenExpiredException()
            :base("Session expired. Please log in again")
        {
        }
    }
}
