using Alakai.FestivalManager.Admin.Contracts.PassTypes.DTOs;
using System.Collections.Generic;

namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses;

public class GetPassTypesResponse
{
    public IReadOnlyList<PassTypeDto> PassTypes { get; set; } = new List<PassTypeDto>();
}
