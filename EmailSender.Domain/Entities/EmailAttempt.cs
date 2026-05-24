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
}
