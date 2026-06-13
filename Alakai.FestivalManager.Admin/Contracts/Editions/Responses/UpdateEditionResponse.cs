using Alakai.FestivalManager.Admin.Contracts.Editions.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class UpdateEditionResponse
{
    public EditionDto Edition { get; set; } = new EditionDto();
}
