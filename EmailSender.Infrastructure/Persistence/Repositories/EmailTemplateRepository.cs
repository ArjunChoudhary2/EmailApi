using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmailSender.Infrastructure.Persistence.Repositories;

public sealed class EmailTemplateRepository(EmailSenderDbContext dbContext) : IEmailTemplateRepository
{
    public Task AddAsync(EmailTemplate template, CancellationToken cancellationToken) =>
        dbContext.EmailTemplates.AddAsync(template, cancellationToken).AsTask();

    public async Task<IReadOnlyList<EmailTemplate>> ListForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.EmailTemplates
            .AsNoTracking()
            .Where(template => template.UserId == userId)
            //.OrderByDescending(attempt => attempt.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<EmailTemplate> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken) =>
        await dbContext.EmailTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(template => template.Id == templateId, cancellationToken);

    public async Task<List<EmailTemplate>> GetAllTemplates(CancellationToken cancellationToken)
    {
        var templates = await dbContext.EmailTemplates.ToListAsync();
        return templates;
    }

}


