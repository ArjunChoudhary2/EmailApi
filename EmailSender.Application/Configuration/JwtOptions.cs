namespace EmailSender.Application.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public required string Secret { get; init; }
    public string Issuer { get; init; } = "EmailSender.Api";
    public string Audience { get; init; } = "EmailSender.Frontend";
    public int ExpiresMinutes { get; init; } = 60;
}
