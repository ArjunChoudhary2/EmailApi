namespace EmailSender.Domain.Entities;

public sealed class EmailAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public required string RecipientEmail { get; set; }
    public required string Subject { get; set; }
    public required string Message { get; set; }
    public EmailAttemptStatus Status { get; set; } = EmailAttemptStatus.Sending;
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SentAt { get; set; }
    public DateTimeOffset? FailedAt { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    
    /// <summary>
    /// ID of the email template used to generate this email (e.g., "delivery-reminder").
    /// Null if this email was created manually (not from a template).
    /// </summary>
    public string? TemplateId { get; set; }
    
    /// <summary>
    /// JSON string containing the field values used to generate this email from a template.
    /// Example: {"poNumber":"PO-123","poDate":"2026-05-29","deliveryDate":"2026-06-10","remarks":"DELIVERY EXTENSION REQUIRED"}
    /// </summary>
    public string? TemplateFieldValuesJson { get; set; }
}
