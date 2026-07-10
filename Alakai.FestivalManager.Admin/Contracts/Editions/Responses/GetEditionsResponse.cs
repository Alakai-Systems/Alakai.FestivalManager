using System.Collections.Generic;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class GetEditionsResponse
{
    public IReadOnlyList<EditionDto> Editions { get; set; } = new List<EditionDto>();
}
