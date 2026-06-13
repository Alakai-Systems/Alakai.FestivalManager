using System;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.Responses;

public class DeleteEditionResponse
{
    public Guid Id { get; set; }

    public bool Deleted { get; set; }
}
