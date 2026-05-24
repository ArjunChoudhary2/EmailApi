namespace EmailSender.Application.Configuration;

public sealed class GoogleOAuthOptions
{
    public const string SectionName = "Google";

    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUri { get; init; }
}
