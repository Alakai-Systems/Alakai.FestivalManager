using System;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class RemoveEditionScheduleResponse
{
    public Guid EditionId { get; set; }

    public bool Removed { get; set; }
}