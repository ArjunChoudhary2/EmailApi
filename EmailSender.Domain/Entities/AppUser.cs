namespace EmailSender.Domain.Entities;

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string GoogleSubject { get; set; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }
    public string? PictureUrl { get; set; }
    public string? EncryptedRefreshToken { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<EmailAttempt> EmailAttempts { get; set; } = new List<EmailAttempt>();
    public ICollection<EmailTemplate> EmailTemplates { get; set; } = [];
}
