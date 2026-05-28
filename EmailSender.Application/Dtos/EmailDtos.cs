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

    [Required]
    public DateTimeOffset ScheduledAt { get; init; }

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
