using Alakai.FestivalManager.Admin.Contracts.Levels.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Levels.Responses;

public class GetLevelByIdResponse
{
    public LevelDto Level { get; set; } = new LevelDto();
}
