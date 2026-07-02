namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Services;

public interface IEmailLayoutService
{
    Task<ApiResponse<GetEmailLayoutsResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateEmailLayoutResponse>> CreateAsync(CreateEmailLayoutRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateEmailLayoutResponse>> UpdateAsync(Guid id, UpdateEmailLayoutRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteEmailLayoutResponse>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}