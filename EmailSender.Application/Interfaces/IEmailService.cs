using EmailSender.Api.Controllers;
using EmailSender.Application.Dtos;
using EmailSender.Application.Services;

namespace EmailSender.Application.Interfaces;

public interface IEmailService
{
    Task<EmailAttemptDto> SendAsync(Guid userId, SendEmailRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmailAttemptDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken);
    Task ScheduleAsync(Guid userId, SendEmailRequest request, CancellationToken cancellationToken);
    Task<EmailTemplateDto> CreateTemplateAsync(Guid userId, CreateEmailTemplateRequest request, CancellationToken cancellationToken);
    Task<EmailTemplateDto> GetTemplateByIdAsync(Guid userId, Guid templateId, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmailTemplateDto>> GetAllSystemTemplatesAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Schedule a template-based email for delivery.
    /// Creates one EmailAttempt entry per recipient with the template field values.
    /// </summary>
    Task<ScheduledEmailResponse> ScheduleTemplateEmailAsync(
        Guid userId,
        ScheduleTemplateEmailRequest request,
        EmailGenerationService emailGenerationService,
        CancellationToken cancellationToken);
}
