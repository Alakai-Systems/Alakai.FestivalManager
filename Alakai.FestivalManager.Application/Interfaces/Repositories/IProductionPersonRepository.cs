namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IProductionPersonRepository
{
    Task<IReadOnlyList<ProductionPerson>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductionPerson?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionPerson>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEditionAndDocumentNumberAsync(Guid editionId, string documentNumber, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionPerson productionPerson, CancellationToken cancellationToken = default);
    void Update(ProductionPerson productionPerson);
    void Delete(ProductionPerson productionPerson);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}