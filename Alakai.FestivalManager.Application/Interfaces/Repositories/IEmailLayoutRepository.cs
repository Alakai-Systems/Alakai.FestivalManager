namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IEmailLayoutRepository
{
    Task<EmailLayout?> GetAsync(CancellationToken cancellationToken = default);
    Task AddAsync(EmailLayout emailLayout, CancellationToken cancellationToken = default);
    void Update(EmailLayout emailLayout);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}