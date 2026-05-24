using System.Security.Cryptography;
using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;

namespace EmailSender.Application.Services;

public sealed class AuthService(
    IGoogleOAuthService googleOAuthService,
    IUserRepository users,
    ITokenProtector tokenProtector,
    IJwtTokenService jwtTokenService,
    IUnitOfWork unitOfWork) : IAuthService
{
    public GoogleLoginUrlResponse CreateGoogleLoginUrl()
    {
        var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return new GoogleLoginUrlResponse(googleOAuthService.BuildAuthorizationUrl(state), state);
    }

    public async Task<AuthResponse> CompleteGoogleLoginAsync(GoogleCallbackRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
        {
            throw new InvalidOperationException("Google authorization code is required.");
        }

        var tokenResult = await googleOAuthService.ExchangeCodeAsync(request.Code, cancellationToken);
        var userInfo = await googleOAuthService.GetUserInfoAsync(tokenResult.AccessToken, cancellationToken);
        var user = await users.GetByGoogleSubjectAsync(userInfo.Subject, cancellationToken);

        if (user is null)
        {
            user = new AppUser
            {
                GoogleSubject = userInfo.Subject,
                Email = userInfo.Email,
                DisplayName = userInfo.Name,
                PictureUrl = userInfo.PictureUrl,
                EncryptedRefreshToken = tokenResult.RefreshToken is null ? null : tokenProtector.Protect(tokenResult.RefreshToken)
            };

            await users.AddAsync(user, cancellationToken);
        }
        else
        {
            user.Email = userInfo.Email;
            user.DisplayName = userInfo.Name;
            user.PictureUrl = userInfo.PictureUrl;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            if (!string.IsNullOrWhiteSpace(tokenResult.RefreshToken))
            {
                user.EncryptedRefreshToken = tokenProtector.Protect(tokenResult.RefreshToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var profile = new UserProfileDto(user.Id, user.Email, user.DisplayName, user.PictureUrl);
        var jwt = jwtTokenService.CreateToken(profile);

        return new AuthResponse(jwt.Token, jwt.ExpiresAt, profile);
    }
}
