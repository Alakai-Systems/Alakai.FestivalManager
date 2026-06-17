namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IEmailLogRepository
{
    Task AddAsync(EmailLog emailLog, CancellationToken cancellationToken = default);
    Task<EmailLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailLog>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailLog>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailLog>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Update(EmailLog emailLog);
    void Delete(EmailLog emailLog);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
