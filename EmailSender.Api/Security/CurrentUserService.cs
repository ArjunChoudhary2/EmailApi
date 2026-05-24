using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmailSender.Application.Interfaces;

namespace EmailSender.Api.Security;

public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("JWT subject is missing or invalid.");
        }
    }
}
