using System;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class SetEditionScheduleResponse
{
    public Guid EditionId { get; set; }

    public string ScheduleUrl { get; set; } = string.Empty;
}