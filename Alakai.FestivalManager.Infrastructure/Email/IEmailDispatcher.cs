namespace Alakai.FestivalManager.Infrastructure.Email;

public interface IEmailDispatcher
{
    Task<bool> SendEmailLogAsync(Guid emailLogId, CancellationToken cancellationToken = default);
}