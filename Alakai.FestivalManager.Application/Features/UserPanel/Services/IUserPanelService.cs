using Alakai.FestivalManager.Application.Features.UserPanel.Contracts.Requests;

namespace Alakai.FestivalManager.Application.Features.UserPanel.Services;

public interface IUserPanelService
{
    Task<ApiResponse<GetUserPanelDashboardResponse>> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateProfileAsync(Guid userId, UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default);
}