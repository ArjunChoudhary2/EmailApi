using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailSender.Application.Services
{
    public sealed class SchedulerService(IEmailAttemptRepository emailAttempt,
            IGmailSender gmailSender,
            IUserRepository users,
            ITokenProtector tokenProtector,
            IUnitOfWork unitOfWork
        )
    {
        public async Task TriggerScheduledEmailsAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var scheduledEmails = await emailAttempt.ListScheduledAsync(cancellationToken,now);

            foreach (var attempt in scheduledEmails)
            {
                var user = await users.GetByIdAsync(attempt.UserId, cancellationToken);
                if (user == null)
                {
                    attempt.Status = EmailAttemptStatus.Failed;
                    attempt.FailedAt = DateTimeOffset.UtcNow;
                    attempt.ErrorMessage = "User not found";
                    continue;
                }
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
            }
           await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
