using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailSender.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profile")]
public sealed class ProfileController(IUserRepository users, ICurrentUserService currentUser) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> Get(CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(currentUser.UserId, cancellationToken);
        return user is null
            ? NotFound()
            : Ok(new UserProfileDto(user.Id, user.Email, user.DisplayName, user.PictureUrl));
    }
}
