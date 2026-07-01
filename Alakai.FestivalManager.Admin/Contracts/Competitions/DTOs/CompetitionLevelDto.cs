namespace Alakai.FestivalManager.Admin.Contracts.Competitions.DTOs;

public class CompetitionLevelDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
