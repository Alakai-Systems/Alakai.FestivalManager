namespace Alakai.FestivalManager.Application.Features.PassTypes.Contracts.Requests;

public class CreatePassTypeRequest
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}