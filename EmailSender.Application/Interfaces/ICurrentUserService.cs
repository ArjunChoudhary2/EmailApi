namespace EmailSender.Application.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
}
