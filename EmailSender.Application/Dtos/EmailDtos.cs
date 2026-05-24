using System.ComponentModel.DataAnnotations;

namespace EmailSender.Application.Dtos;

public sealed class SendEmailRequest : IValidatableObject
{
    [Required]
    [EmailAddress]
    [StringLength(320)]
    public string RecipientEmail { get; init; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Subject { get; init; } = string.Empty;

    [Required]
    [StringLength(20000, MinimumLength = 1)]
    public string Message { get; init; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(RecipientEmail))
        {
            yield return new ValidationResult("Recipient email is required.", [nameof(RecipientEmail)]);
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
    string RecipientEmail,
    string Subject,
    string Message,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? SentAt,
    DateTimeOffset? FailedAt,
    string? ErrorMessage);
