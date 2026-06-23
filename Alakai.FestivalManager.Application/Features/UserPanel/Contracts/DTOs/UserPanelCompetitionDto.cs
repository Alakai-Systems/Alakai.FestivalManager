namespace Alakai.FestivalManager.Application.Features.UserPanel.Contracts.DTOs;

public class UserPanelCompetitionDto
{
    public Guid Id { get; set; }
    public string CompetitionName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public string Status { get; set; } = string.Empty;
}
