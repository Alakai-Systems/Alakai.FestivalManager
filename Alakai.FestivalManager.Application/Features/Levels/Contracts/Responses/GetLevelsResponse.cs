namespace Alakai.FestivalManager.Application.Features.Levels.Contracts.Responses;

public class GetLevelsResponse
{
    public IReadOnlyList<LevelDto> Levels { get; set; } = [];
}
