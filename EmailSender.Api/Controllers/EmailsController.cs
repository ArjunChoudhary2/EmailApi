using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;
using EmailSender.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailSender.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/emails")]
public sealed class EmailsController(
    IEmailService emailService,
    ICurrentUserService currentUser,
    SchedulerService schedulerService,
    EmailGenerationService emailGenerationService) : ControllerBase
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

    [HttpPost("schedule-email")]
    [ProducesResponseType(typeof(EmailAttemptDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Schedule([FromBody] SendEmailRequest request, CancellationToken cancellationToken)
    {
        await emailService.ScheduleAsync(currentUser.UserId, request, cancellationToken);
        return Ok();
    }


    [HttpPost("create-template")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmailTemplateDto>> CreateTemplate([FromBody] CreateEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = await emailService.CreateTemplateAsync(currentUser.UserId, request, cancellationToken);
        return Ok(template);
    }

    [HttpGet("get-template-by-id")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmailTemplateDto>> GetTemplateById(Guid templateId, CancellationToken cancellationToken)
    {
        var template = await emailService.GetTemplateByIdAsync(currentUser.UserId, templateId, cancellationToken);
        if (template == null)
        {
            return NotFound();
        }
        return Ok(template);
    }

    [HttpGet("list-templates")]
    [ProducesResponseType(typeof(IReadOnlyList<EmailTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmailTemplateDto>>> ListTemplates(CancellationToken cancellationToken)
    {
        var templates = await emailService.GetAllSystemTemplatesAsync(cancellationToken);
        return Ok(templates);
    }

    /// <summary>
    /// Generate a preview of an email from a template.
    /// This endpoint is stateless and does not create any email attempts.
    /// </summary>
    [HttpPost("preview")]
    [ProducesResponseType(typeof(EmailGenerationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<EmailGenerationResponse> PreviewEmail([FromBody] EmailGenerationRequest request)
    {
        try
        {
            var response = emailGenerationService.Generate(request.TemplateId, request.FieldValues);
            return Ok(response);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Template not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Schedule a template-based email for delivery at a specific time.
    /// Creates one EmailAttempt entry per recipient with the template field values and scheduled send time.
    /// </summary>
    [HttpPost("scheduled")]
    [ProducesResponseType(typeof(ScheduledEmailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ScheduledEmailResponse> ScheduleTemplateEmail(
        [FromBody] ScheduleTemplateEmailRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = emailService.ScheduleTemplateEmailAsync(
                currentUser.UserId,
                request,
                emailGenerationService,
                cancellationToken).Result;
            return CreatedAtAction(nameof(History), new { }, result);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Template not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}