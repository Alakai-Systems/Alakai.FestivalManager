namespace Alakai.FestivalManager.Application.Features.UserPanel.Contracts.DTOs;

public class UserPanelDashboardDto
{
    public UserPanelUserDto User { get; set; } = default!;
    public UserPanelRegistrationDto? Registration { get; set; }
    public IReadOnlyList<UserPanelCompetitionDto> Competitions { get; set; } = [];
    public IReadOnlyList<UserPanelInvoiceDto> Invoices { get; set; } = [];
}