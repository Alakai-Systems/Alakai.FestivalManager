namespace Alakai.FestivalManager.Application.Features.PassTypes.Contracts.DTOs;

public class PassTypeDto
{
    public Guid Id { get; set; }
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
