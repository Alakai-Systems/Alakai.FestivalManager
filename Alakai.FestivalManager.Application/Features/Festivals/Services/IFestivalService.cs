using Alakai.FestivalManager.Application.Common.Responses;

namespace Alakai.FestivalManager.Application.Features.Festivals.Services;

public interface IFestivalService
{
    Task<ApiResponse<CreateFestivalResponse>> CreateAsync(CreateFestivalCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetFestivalByIdResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetFestivalsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
}
