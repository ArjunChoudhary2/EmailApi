using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;
using EmailSender.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailSender.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/emails")]
public sealed class EmailsController(IEmailService emailService, ICurrentUserService currentUser, SchedulerService schedulerService) : ControllerBase
{
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<EmailAttemptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmailAttemptDto>>> History(CancellationToken cancellationToken)
    {
        var history = await emailService.GetHistoryAsync(currentUser.UserId, cancellationToken);
        return Ok(history);
    }

    [HttpPost("send")]
    [ProducesResponseType(typeof(EmailAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmailAttemptDto>> Send([FromBody] SendEmailRequest request, CancellationToken cancellationToken)
    {
        var attempt = await emailService.SendAsync(currentUser.UserId, request, cancellationToken);
        return Ok(attempt);
    }

    [HttpPost("schedule")]
    [ProducesResponseType(typeof(EmailAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Schedule([FromBody] SendEmailRequest request, CancellationToken cancellationToken)
    {
        await emailService.ScheduleAsync(currentUser.UserId, request, cancellationToken);
        return Ok();
    }

}
