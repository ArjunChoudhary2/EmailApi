using EmailSender.Domain.Entities;

namespace EmailSender.Application.Interfaces;

public interface IEmailAttemptRepository
{
    Task AddAsync(EmailAttempt attempt, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmailAttempt>> ListForUserAsync(Guid userId, CancellationToken cancellationToken);
}
