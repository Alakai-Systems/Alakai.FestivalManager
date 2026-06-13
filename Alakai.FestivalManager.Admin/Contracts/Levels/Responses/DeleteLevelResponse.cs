using System;

namespace Alakai.FestivalManager.Admin.Contracts.Levels.Responses;

public class DeleteLevelResponse
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
}
