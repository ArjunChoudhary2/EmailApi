using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;
using EmailSender.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailSender.Api.Controllers;

[ApiController]
[Route("api/emails")]
public sealed class SchedulerController(
    SchedulerService schedulerService,
    IConfiguration configuration) : ControllerBase
{
    [HttpPost("trigger")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> TriggerScheduledEmails([FromHeader(Name = "X-Trigger-Source")] string triggerSource,
        CancellationToken cancellationToken)
    {
        var header = Request.Headers["X-Trigger-Source"].FirstOrDefault();

        var expectedSecret = configuration["Cron:Secret"];

        if (string.IsNullOrWhiteSpace(header) ||
            header != expectedSecret)
        {
            return Unauthorized();
        }

        await schedulerService.TriggerScheduledEmailsAsync(cancellationToken);

        return Ok();
    }
}