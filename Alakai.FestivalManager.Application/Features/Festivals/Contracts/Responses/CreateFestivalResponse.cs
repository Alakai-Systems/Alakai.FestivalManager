namespace Alakai.FestivalManager.Application.Features.Festivals.Contracts.Responses;
public class CreateFestivalResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}