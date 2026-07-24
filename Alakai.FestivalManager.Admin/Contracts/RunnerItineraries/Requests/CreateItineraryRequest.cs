namespace Alakai.FestivalManager.Admin.Contracts.RunnerItineraries.Requests;

public class CreateItineraryRequest
{
    public Guid EditionId { get; set; }
    public DateTime DateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public int Direction { get; set; }
    public string? RunnerName { get; set; }
    public string? Notes { get; set; }
}