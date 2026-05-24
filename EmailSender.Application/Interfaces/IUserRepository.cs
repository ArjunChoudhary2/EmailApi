using EmailSender.Domain.Entities;

namespace EmailSender.Application.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> GetByGoogleSubjectAsync(string googleSubject, CancellationToken cancellationToken);
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(AppUser user, CancellationToken cancellationToken);
}
