namespace EmailSender.Application.Dtos;

public sealed record GoogleLoginUrlResponse(string AuthorizationUrl, string State);

public sealed record GoogleCallbackRequest(string Code, string State);

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, UserProfileDto User);

public sealed record UserProfileDto(Guid Id, string Email, string? DisplayName, string? PictureUrl);
