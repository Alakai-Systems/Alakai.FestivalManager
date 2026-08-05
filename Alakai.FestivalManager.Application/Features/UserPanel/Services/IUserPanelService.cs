namespace Alakai.FestivalManager.Application.Features.UserPanel.Services;

public interface IUserPanelService
{
    Task<ApiResponse<GetUserPanelDashboardResponse>> GetDashboardAsync(Guid userId, string? domain, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateProfileAsync(Guid userId, UpdateUserPanelProfileRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateCompetitionEntryAsync(Guid userId, CreateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> UpdateCompetitionEntryAsync(Guid userId, Guid competitionEntryId, UpdateCompetitionEntryRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> DeleteCompetitionEntryAsync(Guid userId, Guid competitionEntryId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetUserPanelDashboardResponse>> CreateInvoiceAsync(Guid userId, CreateUserPanelInvoiceRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetMealPreferenceResponse>> GetMealPreferenceAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<SaveMealPreferenceResponse>> SaveMealPreferenceAsync(Guid userId, SaveMealPreferenceCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<RegistrationFestivalInfoDto>> GetFestivalModulesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> GetBusReservationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusesResponse>> GetAvailableBusesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetBusReservationsResponse>> CreateBusReservationsAsync(Guid userId, CreateBusReservationsCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateBusReservationResponse>> UpdateBusReservationAsync(Guid userId, Guid reservationId, UpdateBusReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteBusReservationResponse>> DeleteBusReservationAsync(Guid userId, Guid reservationId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationReservationResponse>> GetAccommodationReservationAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingsResponse>> GetAvailableAccommodationsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<GetAccommodationBuildingResponse>> GetAccommodationBuildingAsync(Guid buildingId, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationReservationResponse>> CreateAccommodationReservationAsync(Guid userId, CreateAccommodationReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<CreateAccommodationReservationResponse>> UpdateAccommodationReservationAsync(Guid userId, Guid reservationId, UpdateAccommodationReservationCommand command, CancellationToken cancellationToken = default);
    Task<ApiResponse<DeleteAccommodationReservationResponse>> DeleteAccommodationReservationAsync(Guid userId, Guid reservationId, CancellationToken cancellationToken = default);
}