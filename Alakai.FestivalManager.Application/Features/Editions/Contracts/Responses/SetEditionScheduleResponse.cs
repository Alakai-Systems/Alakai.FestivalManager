namespace Alakai.FestivalManager.Application.Features.Editions.Contracts.Responses;

public class SetEditionScheduleResponse
{
    public Guid EditionId { get; set; }
    public string ScheduleUrl { get; set; } = string.Empty;
}