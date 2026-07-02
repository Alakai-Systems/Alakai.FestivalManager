namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IEmailLayoutRepository
{
    Task<EmailLayout?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailLayout>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EmailLayout?> GetForEditionAsync(Guid? editionId, CancellationToken cancellationToken = default);
    Task AddAsync(EmailLayout emailLayout, CancellationToken cancellationToken = default);
    void Update(EmailLayout emailLayout);
    void Delete(EmailLayout emailLayout);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}