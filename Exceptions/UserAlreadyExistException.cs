namespace Exceptions
{
    public class UserAlreadyExistException : UIException
    {
        public UserAlreadyExistException()
            :base("User already exist")
        {
        }
    }
}
