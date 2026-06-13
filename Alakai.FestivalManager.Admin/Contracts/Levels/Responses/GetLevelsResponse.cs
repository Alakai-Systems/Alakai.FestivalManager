using Alakai.FestivalManager.Admin.Contracts.Levels.DTOs;
using System.Collections.Generic;

namespace Alakai.FestivalManager.Admin.Contracts.Levels.Responses;

public class GetLevelsResponse
{
    public IReadOnlyList<LevelDto> Levels { get; set; } = new List<LevelDto>();
}
