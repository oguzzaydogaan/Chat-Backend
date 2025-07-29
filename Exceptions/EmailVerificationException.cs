namespace Exceptions
{
    public class EmailVerificationException : Exception
    {
        public EmailVerificationException()
            :base("You must verify your email")
        {
        }
    }
}
