namespace Exceptions
{
    public class TokenExpiredException : UIException
    {
        public TokenExpiredException()
            :base("Session expired. Please log in again")
        {
        }
    }
}
