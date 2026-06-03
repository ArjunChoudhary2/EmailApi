using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EmailSender.Application.Services
{
    public sealed class SchedulerService(IEmailAttemptRepository emailAttempt,
            IGmailSender gmailSender,
            IUserRepository users,
            ITokenProtector tokenProtector,
            IUnitOfWork unitOfWork,
            ITemplateRegistry templateRegistry
        )
    {
        public async Task TriggerScheduledEmailsAsync(CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var scheduledEmails = await emailAttempt.ListScheduledAsync(cancellationToken, now);

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

                // If this email was generated from a template, regenerate it on send for consistency
                string finalSubject = attempt.Subject;
                string finalMessage = attempt.Message;

                if (!string.IsNullOrEmpty(attempt.TemplateId) && !string.IsNullOrEmpty(attempt.TemplateFieldValuesJson))
                {
                    try
                    {
                        var template = templateRegistry.GetTemplate(attempt.TemplateId);
                        if (template != null)
                        {
                            var fieldValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(attempt.TemplateFieldValuesJson) ?? new();
                            var result = template.Generate(fieldValues);
                            finalSubject = result.Subject;
                            finalMessage = result.Body;
                        }
                    }
                    catch (Exception ex)
                    {
                        // If regeneration fails, log but continue with stored values
                        attempt.ErrorMessage = $"Template regeneration failed: {ex.Message}";
                    }
                }

                try
                {
                    var refreshToken = user.EncryptedRefreshToken;
                    await gmailSender.SendAsync(user.Email, refreshToken, attempt.RecipientEmail, finalSubject, finalMessage, cancellationToken);
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
