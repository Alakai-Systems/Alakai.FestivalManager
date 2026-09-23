namespace Alakai.FestivalManager.Application.Features.Editions.Contracts.Responses;

public class RemoveEditionScheduleResponse
{
    public Guid EditionId { get; set; }
    public bool Removed { get; set; }
}