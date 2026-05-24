using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailSender.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(EmailSenderDbContext dbContext) : IUserRepository
{
    public Task<AppUser?> GetByGoogleSubjectAsync(string googleSubject, CancellationToken cancellationToken) =>
        dbContext.Users.FirstOrDefaultAsync(user => user.GoogleSubject == googleSubject, cancellationToken);

    public Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Users.FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task AddAsync(AppUser user, CancellationToken cancellationToken) =>
        dbContext.Users.AddAsync(user, cancellationToken).AsTask();
}
