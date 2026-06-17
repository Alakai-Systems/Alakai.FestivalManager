namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IEmailTemplateRepository
{
    Task AddAsync(EmailTemplate emailTemplate, CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetByTemplateKeyAsync(Guid? editionId, EmailTemplateKey templateKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmailTemplate>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEditionAndTemplateKeyAsync(Guid? editionId, EmailTemplateKey templateKey, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<EmailTemplate?> GetByKeyAsync(EmailTemplateKey templateKey, Guid? editionId, CancellationToken cancellationToken = default);
    void Update(EmailTemplate emailTemplate);
    void Delete(EmailTemplate emailTemplate);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
