namespace EmailSender.Application.Interfaces;

public interface IGoogleOAuthService
{
    string BuildAuthorizationUrl(string state);
    Task<GoogleTokenResult> ExchangeCodeAsync(string code, CancellationToken cancellationToken);
    Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken);
}

public sealed record GoogleTokenResult(string AccessToken, string? RefreshToken, int ExpiresIn);

public sealed record GoogleUserInfo(string Subject, string Email, string? Name, string? PictureUrl);
