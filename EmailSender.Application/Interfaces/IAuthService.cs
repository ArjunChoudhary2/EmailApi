using EmailSender.Application.Dtos;

namespace EmailSender.Application.Interfaces;

public interface IAuthService
{
    GoogleLoginUrlResponse CreateGoogleLoginUrl();
    Task<AuthResponse> CompleteGoogleLoginAsync(GoogleCallbackRequest request, CancellationToken cancellationToken);
}
