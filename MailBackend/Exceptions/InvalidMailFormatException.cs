namespace MailBackend.Exceptions
{
    public class InvalidMailFormatException : Exception
    {
        public InvalidMailFormatException(string m) : base(m)
        {

        }
    }
}
