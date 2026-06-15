using System;

namespace Alakai.FestivalManager.Application.Features.Registrations.Commands.DeleteRegistration;

public class DeleteRegistrationCommand
{
    public Guid Id { get; set; }

    public DeleteRegistrationCommand(Guid id)
    {
        Id = id;
    }
}
