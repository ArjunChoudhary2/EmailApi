using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EmailSender.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpGet("google/login")]
    [ProducesResponseType(typeof(GoogleLoginUrlResponse), StatusCodes.Status200OK)]
    public ActionResult<GoogleLoginUrlResponse> GoogleLogin() =>
        Ok(authService.CreateGoogleLoginUrl());

    [HttpPost("google/callback")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> GoogleCallback([FromBody] GoogleCallbackRequest request, CancellationToken cancellationToken)
    {
        var response = await authService.CompleteGoogleLoginAsync(request, cancellationToken);
        return Ok(response);
    }
}
