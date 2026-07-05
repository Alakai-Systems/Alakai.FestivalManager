namespace Alakai.FestivalManager.Application.Interfaces.Repositories;

public interface IMealPreferenceRepository
{
    Task<MealPreference?> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MealPreference>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task AddAsync(MealPreference preference, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}