using Application.Interfaces;
using Application.Settings;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _emailSettings;
        public EmailService(ILogger<EmailService> logger, EmailSettings emailSettings)
        {
            _logger = logger;
            _emailSettings = emailSettings;
        }
        public async Task SendEmailConfirmationAsync(string email, string userName, string confirmationLink)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, "Movie Recommendation"),
                    Subject = "Confirm Your Email Address",
                    Body = $@"
                        <html>
                        <body>
                            <h2>Welcome {userName}!</h2>
                            <p>Thank you for registering. Please confirm your email address by clicking the link below:</p>
                            <p><a href='{confirmationLink}'>Confirm Email</a></p>
                            <p>Or copy and paste this link into your browser:</p>
                            <p>{confirmationLink}</p>
                            <p>This link will expire in 24 hours.</p>
                        </body>
                        </html>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                using var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
                {
                    Credentials = new NetworkCredential(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Confirmation email sent to: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to: {Email}", email);
                throw;
            }
        }
        public Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            throw new NotImplementedException();
        }
    }
}
