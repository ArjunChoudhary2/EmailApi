namespace EmailSender.Application.Dtos;

/// <summary>
/// Request to schedule a template-based email for delivery.
/// </summary>
public class ScheduleTemplateEmailRequest
{
    /// <summary>
    /// ID of the template to use (e.g., "delivery-reminder").
    /// </summary>
    public required string TemplateId { get; set; }

    /// <summary>
    /// Field values provided by the user for this template.
    /// Keys are field names from the template schema.
    /// </summary>
    public required Dictionary<string, object?> FieldValues { get; set; }

    /// <summary>
    /// Email recipient(s). Can be a single email or semicolon-separated list.
    /// </summary>
    public required List<string> RecipientEmails { get; set; }

    /// <summary>
    /// When to send the email. 
    /// Should be a DateTimeOffset with timezone information (e.g., IST offset +05:30).
    /// Example: "2026-06-25T15:30:00+05:30"
    /// </summary>
    public required DateTimeOffset ScheduledAt { get; set; }
}

/// <summary>
/// Response after successfully scheduling a template-based email.
/// </summary>
public class ScheduledEmailResponse
{
    /// <summary>
    /// List of EmailAttempt IDs created (one per recipient).
    /// </summary>
    public required List<Guid> EmailAttemptIds { get; set; }

    /// <summary>
    /// When the email(s) will be sent (in UTC).
    /// </summary>
    public required DateTimeOffset ScheduledAt { get; set; }

    /// <summary>
    /// The recipients that were scheduled.
    /// </summary>
    public required List<string> Recipients { get; set; }
}
