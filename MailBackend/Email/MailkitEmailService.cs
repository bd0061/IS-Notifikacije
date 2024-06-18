using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
namespace MailBackend.Email
{
    public class MailkitEmailService : IEmailService
    {
        private readonly ILogger<MailkitEmailService> _logger;
        public MailkitEmailService(ILogger<MailkitEmailService> logger) { 
            _logger = logger;
        }
        public void SendEmail(string recipient, string subject, string body)
        {
            var email = new MimeMessage();
            var sender = Environment.GetEnvironmentVariable("MAIL_NAME");
            email.From.Add(MailboxAddress.Parse(sender));
            email.To.Add(MailboxAddress.Parse(recipient));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html) 
            { 
                Text = body
            };

            using var smtp = new SmtpClient();
            smtp.Connect(Environment.GetEnvironmentVariable("SMTP_SERVER"), Convert.ToInt32(Environment.GetEnvironmentVariable("SMTP_PORT")), MailKit.Security.SecureSocketOptions.StartTls);
            smtp.Authenticate(sender, Environment.GetEnvironmentVariable("APP_PASSWORD"));
            smtp.Send(email);
            smtp.Disconnect(true);
        }
    }
}
