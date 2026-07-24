namespace Alakai.FestivalManager.Application.Features.RunnerItineraries.Services;

public interface IRunnerItineraryService
{
    Task<ApiResponse<CreateItineraryResponse>> CreateAsync(CreateItineraryCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetItineraryResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetItinerariesResponse>> GetByEditionIdAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateItineraryResponse>> UpdateAsync(UpdateItineraryCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteItineraryResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}