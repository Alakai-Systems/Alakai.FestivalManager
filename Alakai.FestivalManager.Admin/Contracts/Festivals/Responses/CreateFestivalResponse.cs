using Alakai.FestivalManager.Admin.Contracts.Festivals.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Festivals.Responses;

public class CreateFestivalResponse
{
    public FestivalDto Festival { get; set; } = default!;
}
