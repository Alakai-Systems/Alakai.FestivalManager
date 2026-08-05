namespace Alakai.FestivalManager.Application.Features.UserPanel.Services;

public interface IUserPanelService
{
    Task<ApiResponse<GetUserPanelDashboardResponse>> GetDashboardAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateProfileAsync(Guid userId, string? domain, UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateCompetitionEntryAsync(Guid userId, string? domain, CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateCompetitionEntryAsync(Guid userId, string? domain, Guid competitionEntryId, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> DeleteCompetitionEntryAsync(Guid userId, string? domain, Guid competitionEntryId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateInvoiceAsync(Guid userId, string? domain, CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetMealPreferenceResponse>> GetMealPreferenceAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<SaveMealPreferenceResponse>> SaveMealPreferenceAsync(Guid userId, string? domain, SaveMealPreferenceCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<RegistrationFestivalInfoDto>> GetFestivalModulesAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> GetBusReservationsAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusesResponse>> GetAvailableBusesAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> CreateBusReservationsAsync(Guid userId, string? domain, CreateBusReservationsCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateBusReservationResponse>> UpdateBusReservationAsync(Guid userId, string? domain, Guid reservationId, UpdateBusReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteBusReservationResponse>> DeleteBusReservationAsync(Guid userId, string? domain, Guid reservationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationReservationResponse>> GetAccommodationReservationAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAvailableAccommodationsAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingResponse>> GetAccommodationBuildingAsync(Guid buildingId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationReservationResponse>> CreateAccommodationReservationAsync(Guid userId, string? domain, CreateAccommodationReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationReservationResponse>> UpdateAccommodationReservationAsync(Guid userId, string? domain, Guid reservationId, UpdateAccommodationReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteAccommodationReservationResponse>> DeleteAccommodationReservationAsync(Guid userId, string? domain, Guid reservationId, CancellationToken cancellationToken = default);
}