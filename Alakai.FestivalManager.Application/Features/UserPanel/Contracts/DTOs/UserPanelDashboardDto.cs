namespace Alakai.FestivalManager.Application.Features.UserPanel.Contracts.DTOs;

public class UserPanelDashboardDto
{
    public UserPanelUserDto User { get; set; } = default!;
    public UserPanelRegistrationDto? Registration { get; set; }
    public IReadOnlyList<CompetitionEntryDto> Competitions { get; set; } = [];
    public IReadOnlyList<CompetitionDto> AvailableCompetitions { get; set; } = [];
    public IReadOnlyList<UserPanelInvoiceDto> Invoices { get; set; } = [];
    public IReadOnlyList<CompetitionCapacityDto> CompetitionCapacities { get; set; } = [];
}