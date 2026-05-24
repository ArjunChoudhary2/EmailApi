using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using EmailSender.Application.Configuration;
using EmailSender.Application.Interfaces;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Options;

namespace EmailSender.Infrastructure.Google;

public sealed class GmailSender(HttpClient httpClient, IOptions<GoogleOAuthOptions> options) : IGmailSender
{
    private readonly GoogleOAuthOptions _options = options.Value;

    public async Task SendAsync(string senderEmail, string refreshToken, string recipientEmail, string subject, string message, CancellationToken cancellationToken)
    {
        var accessToken = await RefreshAccessTokenAsync(refreshToken, cancellationToken);
        var credential = GoogleCredential.FromAccessToken(accessToken)
            .CreateScoped(GmailService.Scope.GmailSend);

        using var gmail = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "Email Sender App"
        });

        var gmailMessage = new Message
        {
            Raw = Base64UrlEncode(BuildMimeMessage(senderEmail, recipientEmail, subject, message))
        };

        await gmail.Users.Messages.Send(gmailMessage, "me").ExecuteAsync(cancellationToken);
    }

    private async Task<string> RefreshAccessTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["refresh_token"] = refreshToken,
            ["grant_type"] = "refresh_token"
        }), cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Google refresh token response was empty.");

        return payload.AccessToken;
    }

    private static string BuildMimeMessage(string senderEmail, string recipientEmail, string subject, string message)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"From: {SanitizeHeader(senderEmail)}");
        builder.AppendLine($"To: {SanitizeHeader(recipientEmail)}");
        builder.AppendLine($"Subject: {SanitizeHeader(subject)}");
        builder.AppendLine("MIME-Version: 1.0");
        builder.AppendLine("Content-Type: text/plain; charset=utf-8");
        builder.AppendLine("Content-Transfer-Encoding: 8bit");
        builder.AppendLine();
        builder.AppendLine(message);
        return builder.ToString();
    }

    private static string SanitizeHeader(string value) =>
        value.ReplaceLineEndings(" ").Trim();

    private static string Base64UrlEncode(string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private sealed class RefreshTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; init; } = string.Empty;
    }
}
