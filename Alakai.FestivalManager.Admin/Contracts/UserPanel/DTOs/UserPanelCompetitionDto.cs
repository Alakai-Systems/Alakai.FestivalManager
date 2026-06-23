namespace Alakai.FestivalManager.Admin.Contracts.UserPanel.DTOs;

public class UserPanelCompetitionDto
{
    public Guid Id { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string Status { get; set; } = string.Empty;
}
