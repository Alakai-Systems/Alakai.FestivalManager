using System;

namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Responses;

public class DeletePassTypeResponse
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
}
