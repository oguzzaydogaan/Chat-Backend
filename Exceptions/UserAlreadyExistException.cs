namespace Exceptions
{
    public class UserAlreadyExistException : Exception
    {
        public UserAlreadyExistException()
            :base("User already exist")
        {
        }
    }
}
