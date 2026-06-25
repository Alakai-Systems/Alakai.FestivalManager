namespace Alakai.FestivalManager.Admin.Contracts.UserPanel.DTOs;

public class UserPanelDashboardDto
{
    public UserPanelUserDto User { get; set; } = default!;
    public UserPanelRegistrationDto? Registration { get; set; }
    public IReadOnlyList<CompetitionEntryDto> Competitions { get; set; } = [];
    public IReadOnlyList<CompetitionDto> AvailableCompetitions { get; set; } = [];
    public IReadOnlyList<UserPanelInvoiceDto> Invoices { get; set; } = [];
    public IReadOnlyList<CompetitionCapacityDto> CompetitionCapacities { get; set; } = [];
}