namespace EmailSender.Application.Interfaces;

public interface IGmailSender
{
    Task SendAsync(string senderEmail, string refreshToken, string recipientEmail, string subject, string message, CancellationToken cancellationToken);
}
