using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailSender.Infrastructure.Persistence.Repositories;

public sealed class EmailAttemptRepository(EmailSenderDbContext dbContext) : IEmailAttemptRepository
{
    public Task AddAsync(EmailAttempt attempt, CancellationToken cancellationToken) =>
        dbContext.EmailAttempts.AddAsync(attempt, cancellationToken).AsTask();

    public async Task<IReadOnlyList<EmailAttempt>> ListForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.EmailAttempts
            .AsNoTracking()
            .Where(attempt => attempt.UserId == userId)
            .OrderByDescending(attempt => attempt.CreatedAt)
            .ToListAsync(cancellationToken);
}
