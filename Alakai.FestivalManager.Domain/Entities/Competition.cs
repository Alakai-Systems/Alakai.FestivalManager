namespace Alakai.FestivalManager.Domain.Entities;

public class Competition : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public CompetitionFormat Format { get; set; }
    public MixAndMtachLevel? MixAndMatchLevel { get; set; }

    public int? MaxParticipants { get; set; }

    public bool RequiresPartner { get; set; }
    public bool RequiresRole { get; set; }

    public decimal Price { get; set; }

    public DateTime? RegistrationOpenAt { get; set; }
    public DateTime? RegistrationCloseAt { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<CompetitionEntry> Entries { get; set; } = [];
}
