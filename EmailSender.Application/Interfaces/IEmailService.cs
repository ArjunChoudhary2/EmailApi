using EmailSender.Application.Dtos;

namespace EmailSender.Application.Interfaces;

public interface IEmailService
{
    Task<EmailAttemptDto> SendAsync(Guid userId, SendEmailRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmailAttemptDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken);
    Task ScheduleAsync(Guid userId, SendEmailRequest request, CancellationToken cancellationToken);
}
