using EmailSender.Domain.Entities;

namespace EmailSender.Application.Interfaces;

public interface IEmailTemplateRepository
{
    Task AddAsync(EmailTemplate template, CancellationToken cancellationToken);
    Task<IReadOnlyList<EmailTemplate>> ListForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<EmailTemplate> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken);

    Task<List<EmailTemplate>> GetAllTemplates(CancellationToken cancellationToken);
}
