namespace Alakai.FestivalManager.Application.Features.Buses.Services;

public interface IBusService
{
    Task<ApiResponse<GetBusesResponse>> GetAllAsync(Guid editionId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusesResponse>> GetAvailableForRegistrationAsync(Guid registrationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateBusResponse>> CreateAsync(CreateBusCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateBusResponse>> UpdateAsync(UpdateBusCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteBusResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}