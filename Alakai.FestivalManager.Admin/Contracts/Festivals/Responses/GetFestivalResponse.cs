using Alakai.FestivalManager.Admin.Contracts.Festivals.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Festivals.Responses;

public class GetFestivalsResponse
{
    public IReadOnlyList<FestivalDto> Festivals { get; set; } = [];
}
