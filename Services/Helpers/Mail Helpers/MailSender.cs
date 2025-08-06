using System.Net;
using System.Net.Mail;

namespace Services.Helpers.Mail_Helpers
{
    public class MailSender
    {
        public async Task SendEmailAsync(string email, string token)
        {
            var verificationLink = $"https://api.oguzzaydogaan.com.tr/api/users/verify?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
            MailMessage message = new()
            {
                From = new MailAddress("testchat.0606@gmail.com"),
                Subject = "ChatApp Email Verification",
                Body = $"<p>Please verify your email by clicking the link below:</p><p><a href='{verificationLink}'>Verify Email</a></p>",
                IsBodyHtml = true
            };

            message.To.Add(email);

            using var smtp = new SmtpClient("smtp.gmail.com",587);
            smtp.Credentials = new NetworkCredential("testchat.0606@gmail.com", "eexnecqmmsmmlopo");
            smtp.EnableSsl = true;

            await smtp.SendMailAsync(message);
        }
    }
}
