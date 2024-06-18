namespace MailBackend.Exceptions
{
    public class EmptyMailException : Exception
    {
        public EmptyMailException(string m) : base(m)
        {
            
        }
    }
}
