using Alakai.FestivalManager.Domain.Common;

namespace Alakai.FestivalManager.Domain.Entities;

public class MediaAsset : BaseEntity
{
    public Guid FestivalId { get; set; }
    public Festival Festival { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
}