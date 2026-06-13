using System;

namespace Alakai.FestivalManager.Admin.Contracts.Editions.DTOs;

public class EditionDto
{
    public Guid Id { get; set; }

    public Guid FestivalId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Year { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public DateTime? RegistrationOpenDate { get; set; }

    public DateTime? RegistrationCloseDate { get; set; }

    public bool IsActive { get; set; }
}
