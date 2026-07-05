using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Meals.Commands;
using Alakai.FestivalManager.Application.Features.Meals.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.Meals.Services;

public interface IMealPreferenceService
{
    Task<ApiResponse<GetMealPreferenceResponse>> GetByRegistrationIdAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetMealPreferencesResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SaveMealPreferenceResponse>> SaveAsync(SaveMealPreferenceCommand command, CancellationToken cancellationToken = default);
}