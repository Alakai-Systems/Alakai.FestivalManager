namespace Alakai.FestivalManager.Admin.Contracts.UserPanel.DTOs;

public class UserPanelDashboardDto
{
    public UserPanelUserDto User { get; set; } = default!;
    public UserPanelRegistrationDto? Registration { get; set; }
    public IReadOnlyList<UserPanelCompetitionDto> Competitions { get; set; } = [];
    public IReadOnlyList<UserPanelInvoiceDto> Invoices { get; set; } = [];
}