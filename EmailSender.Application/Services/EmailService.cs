using EmailSender.Api.Controllers;
using EmailSender.Application.Dtos;
using EmailSender.Application.Interfaces;
using EmailSender.Domain.Entities;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace EmailSender.Application.Services;

public sealed class EmailService(
    IUserRepository users,
    IEmailAttemptRepository attempts,
    IGmailSender gmailSender,
    ITokenProtector tokenProtector,
    IUnitOfWork unitOfWork,
    IEmailTemplateRepository templates) : IEmailService
{
    public async Task<EmailAttemptDto> SendAsync(Guid userId, SendEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Authenticated user was not found.");

        if (string.IsNullOrWhiteSpace(user.EncryptedRefreshToken))
        {
            throw new InvalidOperationException("Gmail send permission is missing. Sign in with Google again and grant Gmail send access.");
        }

        var attempt = new EmailAttempt
        {
            UserId = user.Id,
            RecipientEmail = request.RecipientEmails.First().Trim(),
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Status = EmailAttemptStatus.Sending
        };

        await attempts.AddAsync(attempt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var refreshToken = user.EncryptedRefreshToken;
            await gmailSender.SendAsync(user.Email, refreshToken, attempt.RecipientEmail, attempt.Subject, attempt.Message, cancellationToken);
            attempt.Status = EmailAttemptStatus.Sent;
            attempt.SentAt = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            attempt.Status = EmailAttemptStatus.Failed;
            attempt.FailedAt = DateTimeOffset.UtcNow;
            attempt.ErrorMessage = ex.Message;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(attempt);
    }

    public async Task ScheduleAsync(Guid userId, SendEmailRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Authenticated user was not found.");

        if (string.IsNullOrWhiteSpace(user.EncryptedRefreshToken))
        {
            throw new InvalidOperationException("Gmail send permission is missing. Sign in with Google again and grant Gmail send access.");
        }

        foreach (var recipient in request.RecipientEmails)
        {
            if (string.IsNullOrWhiteSpace(recipient))
            {
                throw new InvalidOperationException("Recipient email cannot be empty.");
            }
            var attempt = new EmailAttempt
            {
                UserId = user.Id,
                RecipientEmail = recipient.Trim(),
                Subject = request.Subject.Trim(),
                Message = request.Message.Trim(),
                Status = EmailAttemptStatus.Scheduled,
                ScheduledAt = request.ScheduledAt
            };
            await attempts.AddAsync(attempt, cancellationToken);
        }
        

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EmailAttemptDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken)
    {
        var history = await attempts.ListForUserAsync(userId, cancellationToken);
        return history.Select(Map).ToList();
    }

    private static EmailAttemptDto Map(EmailAttempt attempt) =>
        new(attempt.Id, new List<String> { attempt.RecipientEmail }, attempt.Subject, attempt.Message, attempt.Status.ToString(), attempt.CreatedAt, attempt.SentAt, attempt.FailedAt, attempt.ErrorMessage);

    public async Task<EmailTemplateDto> CreateTemplateAsync(Guid userId, CreateEmailTemplateRequest request, CancellationToken cancellationToken)
    {
        // Validate the form data
        // Create a new EmailTemplate entity
        // Save the template to the database
        // Return the created template as a DTO
        if(request.ReminderOffsets.Any(offset => offset < 0))
        {
          throw new InvalidOperationException("Reminder offsets cannot be negative.");
        }
        EmailTemplate template = new();
        template.Id = Guid.NewGuid();
        template.UserId = userId;
        template.TemplateName = request.TemplateName.Trim();
        template.SubjectTemplate = request.SubjectTemplate.Trim();
        template.BodyTemplate = request.BodyTemplate.Trim();
        template.Description = request.Description?.Trim();
        template.FieldsJson = System.Text.Json.JsonSerializer.Serialize(request.Fields);
        template.ReminderOffsetsJson = System.Text.Json.JsonSerializer.Serialize(request.ReminderOffsets);
        template.IsSystemTemplate = request.IsSystemTemplate;
        template.CreatedAt = DateTime.UtcNow;
        await templates.AddAsync(template, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new EmailTemplateDto
        {
            TemplateName = template.TemplateName,
            SubjectTemplate = template.SubjectTemplate,
            BodyTemplate = template.BodyTemplate,
            Description = template.Description,
            Fields = request.Fields,
            ReminderOffsets = request.ReminderOffsets,
            IsSystemTemplate = template.IsSystemTemplate
        };
    }

    public async Task<EmailTemplateDto> GetTemplateByIdAsync(Guid userId, Guid templateId, CancellationToken cancellationToken)
    {
        var template = await templates.GetTemplateAsync(templateId, cancellationToken);
        if (template == null || template.UserId != userId)
        {
            return null;
        }
        return new EmailTemplateDto
        {
            TemplateName = template.TemplateName,
            SubjectTemplate = template.SubjectTemplate,
            BodyTemplate = template.BodyTemplate,
            Description = template.Description,
            Fields = JsonSerializer.Deserialize<List<TemplateFieldDto>>(template.FieldsJson) ?? [],
            ReminderOffsets = System.Text.Json.JsonSerializer.Deserialize<List<int>>(template.ReminderOffsetsJson),
            IsSystemTemplate = template.IsSystemTemplate
        };
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> GetAllTemplatesForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var templatesForUser = await templates.ListForUserAsync(userId, cancellationToken);
        return templatesForUser.Select(template => new EmailTemplateDto
        {
            TemplateName = template.TemplateName,
            SubjectTemplate = template.SubjectTemplate,
            BodyTemplate = template.BodyTemplate,
            Description = template.Description,
            Fields = JsonSerializer.Deserialize<List<TemplateFieldDto>>(template.FieldsJson) ?? [],
            ReminderOffsets = System.Text.Json.JsonSerializer.Deserialize<List<int>>(template.ReminderOffsetsJson),
            IsSystemTemplate = template.IsSystemTemplate
        }).ToList();
    }

    public async Task<IReadOnlyList<EmailTemplateDto>> GetAllSystemTemplatesAsync(CancellationToken cancellationToken)
    {
        var allTemplates = await templates.GetAllTemplates(cancellationToken);
        var systemTemplates = allTemplates.Where(t => t.IsSystemTemplate).ToList();
        return systemTemplates.Select(template => new EmailTemplateDto
        {
            TemplateName = template.TemplateName,
            SubjectTemplate = template.SubjectTemplate,
            BodyTemplate = template.BodyTemplate,
            Description = template.Description,
            Fields = JsonSerializer.Deserialize<List<TemplateFieldDto>>(template.FieldsJson) ?? [],
            ReminderOffsets = System.Text.Json.JsonSerializer.Deserialize<List<int>>(template.ReminderOffsetsJson),
            IsSystemTemplate = template.IsSystemTemplate
        }).ToList();
    }

    /// <summary>
    /// Schedule a template-based email for delivery.
    /// Creates one EmailAttempt entry per recipient with template field values and scheduled time.
    /// </summary>
    public async Task<ScheduledEmailResponse> ScheduleTemplateEmailAsync(
        Guid userId,
        ScheduleTemplateEmailRequest request,
        EmailGenerationService emailGenerationService,
        CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(userId, cancellationToken)
            ?? throw new InvalidOperationException("Authenticated user was not found.");

        // Validate template exists and field values are valid
        if (!emailGenerationService.ValidateFields(request.TemplateId, request.FieldValues, out var validationErrors))
        {
            throw new ArgumentException($"Invalid field values: {string.Join(", ", validationErrors)}");
        }

        // Parse recipient emails
        var recipients = request.RecipientEmails;

        if (!recipients.Any())
            throw new ArgumentException("At least one recipient email is required");

        // Generate email content using the template (to store the generated subject/body for reference)
        var emailContent = emailGenerationService.Generate(request.TemplateId, request.FieldValues);

        // Serialize field values to JSON for storage
        var fieldValuesJson = JsonSerializer.Serialize(request.FieldValues);

        // Create one EmailAttempt per recipient
        var createdAttemptIds = new List<Guid>();
        foreach (var recipientEmail in recipients)
        {
            if (!TemplateFieldValidators.IsValidEmail(recipientEmail))
                throw new ArgumentException($"Invalid email address: {recipientEmail}");

            var attempt = new EmailAttempt
            {
                UserId = user.Id,
                RecipientEmail = recipientEmail,
                Subject = emailContent.Subject,
                Message = emailContent.Body,
                Status = EmailAttemptStatus.Scheduled,
                ScheduledAt = request.ScheduledAt.ToUniversalTime(),
                TemplateId = request.TemplateId,
                TemplateFieldValuesJson = fieldValuesJson
            };

            await attempts.AddAsync(attempt, cancellationToken);
            createdAttemptIds.Add(attempt.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ScheduledEmailResponse
        {
            EmailAttemptIds = createdAttemptIds,
            ScheduledAt = request.ScheduledAt,
            Recipients = recipients
        };
    }
}