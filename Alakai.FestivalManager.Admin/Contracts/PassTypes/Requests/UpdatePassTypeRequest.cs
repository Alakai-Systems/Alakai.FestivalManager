using System;

namespace Alakai.FestivalManager.Admin.Contracts.PassTypes.Requests;

public class UpdatePassTypeRequest
{
    public Guid EditionId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }
}
