namespace Alakai.FestivalManager.Application.Features.Festivals.Commands.CreateFestival;

public class CreateFestivalCommand
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Website { get; set; }
    public string? LogoUrl { get; set; }
}
