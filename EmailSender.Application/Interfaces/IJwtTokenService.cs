using EmailSender.Application.Dtos;

namespace EmailSender.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(UserProfileDto user);
}
