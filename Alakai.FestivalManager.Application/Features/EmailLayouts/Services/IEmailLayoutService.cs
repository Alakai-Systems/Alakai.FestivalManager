namespace Alakai.FestivalManager.Application.Features.EmailLayouts.Services;

public interface IEmailLayoutService
{
    Task<ApiResponse<GetEmailLayoutResponse>> GetAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<UpdateEmailLayoutResponse>> UpdateAsync(UpdateEmailLayoutRequest request, CancellationToken cancellationToken = default);
}