namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IAccommodationBuildingRepository
{
    Task<IReadOnlyList<AccommodationBuilding>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<AccommodationBuilding?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(AccommodationBuilding building, CancellationToken cancellationToken = default);
    void Update(AccommodationBuilding building);
    void Delete(AccommodationBuilding building);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}