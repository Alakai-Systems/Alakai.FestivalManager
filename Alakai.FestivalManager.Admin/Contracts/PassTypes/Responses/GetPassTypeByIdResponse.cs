using Alakai.FestivalManager.Admin.Contracts.PassTypes.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses;

public class GetPassTypeByIdResponse
{
    public PassTypeDto PassType { get; set; } = new PassTypeDto();
}
