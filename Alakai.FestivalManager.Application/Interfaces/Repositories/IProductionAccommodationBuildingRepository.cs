namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IProductionAccommodationBuildingRepository
{
    Task<IReadOnlyList<ProductionAccommodationBuilding>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductionAccommodationBuilding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProductionAccommodationBuilding>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductionAccommodationBuilding building, CancellationToken cancellationToken = default);
    void Update(ProductionAccommodationBuilding building);
    void Delete(ProductionAccommodationBuilding building);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}