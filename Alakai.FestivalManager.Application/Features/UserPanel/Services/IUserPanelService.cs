namespace Alakai.FestivalManager.Application.Features.UserPanel.Services;

public interface IUserPanelService
{
    Task<ApiResponse<GetUserPanelDashboardResponse>> GetDashboardAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateProfileAsync(Guid userId, UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateCompetitionEntryAsync(Guid userId, CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateCompetitionEntryAsync(Guid userId, Guid competitionEntryId, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> DeleteCompetitionEntryAsync(Guid userId, Guid competitionEntryId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateInvoiceAsync(Guid userId, CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default);
}