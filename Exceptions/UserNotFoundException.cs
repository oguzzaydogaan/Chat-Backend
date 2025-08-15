namespace Exceptions
{
    public class UsersNotFoundException : UIException
    {
        public UsersNotFoundException()
            :base("User not found")
        {
        }
    }
}
