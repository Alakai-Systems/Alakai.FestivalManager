namespace Alakai.FestivalManager.Application.Features.Editions.Commands.SetEditionSchedule;

public class SetEditionScheduleCommand
{
    public Guid EditionId { get; set; }
    public Stream Content { get; set; } = default!;
    public string FileName { get; set; } = string.Empty;

    public SetEditionScheduleCommand(Guid editionId, Stream content, string fileName)
    {
        EditionId = editionId;
        Content = content;
        FileName = fileName;
    }
}