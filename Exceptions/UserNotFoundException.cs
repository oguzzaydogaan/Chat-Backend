namespace Exceptions
{
    public class UsersNotFoundException : Exception
    {
        public UsersNotFoundException()
            :base("User not found")
        {
        }
    }
}
