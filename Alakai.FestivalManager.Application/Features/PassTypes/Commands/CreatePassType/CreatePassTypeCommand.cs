namespace Alakai.FestivalManager.Application.Features.PassTypes.Commands.CreatePassType;

public class CreatePassTypeCommand
{
    public Guid EditionId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}