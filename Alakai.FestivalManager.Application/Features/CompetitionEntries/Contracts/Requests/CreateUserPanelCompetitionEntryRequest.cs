namespace Alakai.FestivalManager.Application.Features.UserPanel.Contracts.Requests;

public class CreateUserPanelCompetitionEntryRequest
{
    public Guid CompetitionId { get; set; }
    public string? Role { get; set; }
}