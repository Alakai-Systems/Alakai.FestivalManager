namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public interface IFestivalCredentialsService
{
    Task<ApiResponse<GetFestivalCredentialsResponse>> GetByFestivalIdAsync(Guid festivalId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpsertFestivalCredentialsResponse>> UpsertAsync(UpsertFestivalCredentialsCommand command, CancellationToken cancellationToken = default);
}