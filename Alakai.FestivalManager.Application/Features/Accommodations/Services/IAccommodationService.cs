using Alakai.FestivalManager.Application.Common.Responses;
using Alakai.FestivalManager.Application.Features.Accommodations.Commands;
using Alakai.FestivalManager.Application.Features.Accommodations.Contracts.Responses;

namespace Alakai.FestivalManager.Application.Features.Accommodations.Services;

public interface IAccommodationService
{
    Task<ApiResponse<CreateAccommodationResponse>> CreateAsync(CreateAccommodationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateAccommodationResponse>> UpdateAsync(UpdateAccommodationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteAccommodationEntityResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}