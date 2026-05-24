using System.Net.Http.Json;
using System.Text.Json.Serialization;
using EmailSender.Application.Configuration;
using EmailSender.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace EmailSender.Infrastructure.Google;

public sealed class GoogleOAuthService(HttpClient httpClient, IOptions<GoogleOAuthOptions> options) : IGoogleOAuthService
{
    private static readonly string[] Scopes =
    [
        "openid",
        "email",
        "profile",
        "https://www.googleapis.com/auth/gmail.send"
    ];

    private readonly GoogleOAuthOptions _options = options.Value;

    public string BuildAuthorizationUrl(string state)
    {
        var query = new Dictionary<string, string?>
        {
            ["client_id"] = _options.ClientId,
            ["redirect_uri"] = _options.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = string.Join(' ', Scopes),
            ["access_type"] = "offline",
            ["prompt"] = "consent",
            ["state"] = state
        };

        return "https://accounts.google.com/o/oauth2/v2/auth?" + string.Join('&',
            query.Select(pair => $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value ?? string.Empty)}"));
    }

    public async Task<GoogleTokenResult> ExchangeCodeAsync(string code, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["code"] = code,
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["redirect_uri"] = _options.RedirectUri,
            ["grant_type"] = "authorization_code"
        }), cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GoogleTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Google token response was empty.");

        return new GoogleTokenResult(payload.AccessToken, payload.RefreshToken, payload.ExpiresIn);
    }

    public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "https://openidconnect.googleapis.com/v1/userinfo");
        request.Headers.Authorization = new("Bearer", accessToken);

        var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GoogleUserInfoResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Google userinfo response was empty.");

        return new GoogleUserInfo(payload.Subject, payload.Email, payload.Name, payload.Picture);
    }

    private sealed class GoogleTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; init; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; init; }
    }

    private sealed class GoogleUserInfoResponse
    {
        [JsonPropertyName("sub")]
        public string Subject { get; init; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; init; } = string.Empty;

        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("picture")]
        public string? Picture { get; init; }
    }
}
