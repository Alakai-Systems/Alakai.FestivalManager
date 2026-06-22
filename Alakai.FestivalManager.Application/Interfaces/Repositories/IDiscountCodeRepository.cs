using Microsoft.EntityFrameworkCore;

namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IDiscountCodeRepository
{
    Task AddAsync(DiscountCode discountCode, CancellationToken cancellationToken = default);
    Task<DiscountCode?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DiscountCode?> GetByEditionAndCodeAsync(Guid editionId, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DiscountCode>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DiscountCode>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEditionAndCodeAsync(Guid editionId, string code, CancellationToken cancellationToken = default);
    Task<int> CountUsesAsync(Guid discountCodeId, CancellationToken cancellationToken = default);
    void Update(DiscountCode discountCode);
    void Delete(DiscountCode discountCode);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
