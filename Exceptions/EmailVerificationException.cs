namespace Exceptions
{
    public class EmailVerificationException : UIException
    {
        public EmailVerificationException()
            :base("You must verify your email")
        {
        }
    }
}
