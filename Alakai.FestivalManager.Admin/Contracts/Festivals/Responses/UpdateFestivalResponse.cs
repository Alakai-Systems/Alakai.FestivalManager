using Alakai.FestivalManager.Admin.Contracts.Festivals.DTOs;

namespace Alakai.FestivalManager.Admin.Contracts.Festivals.Responses;

public class UpdateFestivalResponse
{
    public FestivalDto Festival { get; set; } = default!;
}