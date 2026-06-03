using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace EmailSender.Application.Dtos;

public sealed class SendEmailRequest : IValidatableObject
{
    [Required]
    [MinLength(1)]
    public List<string> RecipientEmails { get; init; } = [];

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Subject { get; init; } = string.Empty;

    [Required]
    [StringLength(20000, MinimumLength = 1)]
    public string Message { get; init; } = string.Empty;

    // Optional: only required for scheduled emails
    public DateTimeOffset? ScheduledAt { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RecipientEmails == null || RecipientEmails.Count == 0)
        {
            yield return new ValidationResult("At least one recipient email is required.", new[] { nameof(RecipientEmails) });
        }

        if (string.IsNullOrWhiteSpace(Subject))
        {
            yield return new ValidationResult("Subject is required.", [nameof(Subject)]);
        }

        if (string.IsNullOrWhiteSpace(Message))
        {
            yield return new ValidationResult("Message is required.", [nameof(Message)]);
        }

        // Check for duplicate emails (case-insensitive)
        if (RecipientEmails != null && RecipientEmails.Count > 0)
        {
            var normalizedEmails = RecipientEmails
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim().ToLowerInvariant())
                .ToList();

            var uniqueEmails = new HashSet<string>(normalizedEmails);

            if (uniqueEmails.Count != normalizedEmails.Count)
            {
                yield return new ValidationResult("Recipient emails contain duplicates.", [nameof(RecipientEmails)]);
            }
        }
    }
}

public sealed record EmailAttemptDto(
    Guid Id,
    List<string> RecipientEmails,
    string Subject,
    string Message,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? FailedAt,
    string? ErrorMessage);
