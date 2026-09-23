namespace Alakai.FestivalManager.Domain.Entities;

public class PassType : BaseEntity
{
    public Guid EditionId { get; set; }
    public Edition Edition { get; set; } = default!;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowsMultipleLevels { get; set; }
    public decimal? AllLevelsDiscountPercent { get; set; }

    // Modulos visibles/activos en el Panel de Usuario para este pase en concreto (Competitions,
    // Accommodation, Transport, Meals -- mismo enum que Festival.EnabledModules). Por defecto los
    // 4 activos para no cambiar el comportamiento de pases ya existentes; los pases nuevos calculan
    // su valor a partir de los modulos del festival (ver CreatePassTypeHandler).
    public FestivalModule EnabledModules { get; set; } = FestivalModule.Competitions | FestivalModule.Accommodation | FestivalModule.Transport | FestivalModule.Meals;

    public ICollection<Level> Levels { get; set; } = new List<Level>();
}
