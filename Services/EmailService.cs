using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace GTBStatementService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendStatementAsync(string receiverEmail, string subject, string body, byte[] attachment, string fileName)
        {
            var server = _configuration["StatementSettings:EmailServer"] ?? "127.0.0.1";
            var port = int.Parse(_configuration["StatementSettings:EmailPort"] ?? "25");
            var fromEmail = _configuration["StatementSettings:PostMasterEmail"] ?? "statement@gtbank.com";
            var fromName = _configuration["StatementSettings:PostMasterName"] ?? "GTBank";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(receiverEmail, receiverEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            
            // Add the attachment
            builder.Attachments.Add(fileName, attachment);
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Connect without SSL as per internal bank SMTP relay typical behavior
                // Adjust to SecureSocketOptions.StartTls if required
                await client.ConnectAsync(server, port, MailKit.Security.SecureSocketOptions.None);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                
                _logger.LogInformation("Email sent successfully to {Email}", receiverEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", receiverEmail);
                throw;
            }
        }

        public async Task SendStatementAsync(string receiverEmail, string subject, string body, IReadOnlyList<byte[]> pdfFiles)
        {
            var server = _configuration["StatementSettings:EmailServer"] ?? "127.0.0.1";
            var port = int.TryParse(_configuration["StatementSettings:EmailPort"], out var p) ? p : 25;
            var fromEmail = _configuration["StatementSettings:PostMasterEmail"] ?? "statement@gtbank.com";
            var fromName = _configuration["StatementSettings:PostMasterName"] ?? "GTBank";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse("vanusquarm@outlook.com"));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };

            // Attach PDFs
            for (int i = 0; i < pdfFiles.Count; i++)
            {
                builder.Attachments.Add(
                    $"Statement_{i + 1}.pdf",
                    pdfFiles[i],
                    new ContentType("application", "pdf"));
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                await client.ConnectAsync(server, port, MailKit.Security.SecureSocketOptions.None);

                if (!string.IsNullOrWhiteSpace(_configuration["StatementSettings:Username"]))
                {
                    await client.AuthenticateAsync(
                        _configuration["StatementSettings:Username"],
                        _configuration["StatementSettings:Password"]);
                }

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Statement email sent to {Email}", receiverEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send statement email to {Email}", receiverEmail);
                throw;
            }
        }
    }
}
