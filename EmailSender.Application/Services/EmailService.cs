using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;

namespace EmailSender.Application.Services;

public sealed class EmailService(
    IUserRepository users,
    IEmailAttemptRepository attempts,
    IGmailSender gmailSender,
    ITokenProtector tokenProtector,
    IUnitOfWork unitOfWork) : IEmailService
{
    public async Task<EmailAttemptDto> SendAsync(Guid userId, SendEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Authenticated user was not found.");

        if (string.IsNullOrWhiteSpace(user.EncryptedRefreshToken))
        {
            throw new InvalidOperationException("Gmail send permission is missing. Sign in with Google again and grant Gmail send access.");
        }

        var attempt = new EmailAttempt
        {
            UserId = user.Id,
            RecipientEmail = request.RecipientEmails.First().Trim(),
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Status = EmailAttemptStatus.Sending
        };

        await attempts.AddAsync(attempt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var refreshToken = user.EncryptedRefreshToken;
            await gmailSender.SendAsync(user.Email, refreshToken, attempt.RecipientEmail, attempt.Subject, attempt.Message, cancellationToken);
            attempt.Status = EmailAttemptStatus.Sent;
            attempt.SentAt = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            attempt.Status = EmailAttemptStatus.Failed;
            attempt.FailedAt = DateTimeOffset.UtcNow;
            attempt.ErrorMessage = ex.Message;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(attempt);
    }

    public async Task ScheduleAsync(Guid userId, SendEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Authenticated user was not found.");

        if (string.IsNullOrWhiteSpace(user.EncryptedRefreshToken))
        {
            throw new InvalidOperationException("Gmail send permission is missing. Sign in with Google again and grant Gmail send access.");
        }

        foreach (var recipient in request.RecipientEmails)
        {
            if (string.IsNullOrWhiteSpace(recipient))
            {
                throw new InvalidOperationException("Recipient email cannot be empty.");
            }
            var attempt = new EmailAttempt
            {
                UserId = user.Id,
                RecipientEmail = recipient.Trim(),
                Subject = request.Subject.Trim(),
                Message = request.Message.Trim(),
                Status = EmailAttemptStatus.Scheduled,
                ScheduledAt = request.ScheduledAt
            };
            await attempts.AddAsync(attempt, cancellationToken);
        }
        

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailAttemptDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var history = await attempts.ListForUserAsync(userId, cancellationToken);
        return history.Select(Map).ToList();
    }

    private static EmailAttemptDto Map(EmailAttempt attempt) =>
        new(attempt.Id, new List<String> { attempt.RecipientEmail }, attempt.Subject, attempt.Message, attempt.Status.ToString(), attempt.CreatedAt, attempt.SentAt, attempt.FailedAt, attempt.ErrorMessage);
}
