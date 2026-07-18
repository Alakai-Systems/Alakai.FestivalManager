using Alakai.FestivalManager.Application.Features.Files.Services;
using Alakai.FestivalManager.Application.Interfaces.Repositories;
using Alakai.FestivalManager.Domain.Entities;

namespace Alakai.FestivalManager.Api.Controllers;

public class UploadImageForm
{
    public IFormFile File { get; set; } = default!;
    public int? Width { get; set; }
    public Guid? FestivalId { get; set; }
}

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IMediaAssetRepository _mediaAssetRepository;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/gif", "image/webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadsController(IFileStorageService fileStorageService, IMediaAssetRepository mediaAssetRepository)
    {
        _fileStorageService = fileStorageService;
        _mediaAssetRepository = mediaAssetRepository;
    }

    [HttpPost("images")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> UploadImage([FromForm] UploadImageForm form, CancellationToken cancellationToken)
    {
        IFormFile file = form.File;
        int? width = form.Width;

        if (file is null || file.Length == 0)
        {
            return BadRequest(new { error = "No file was provided." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { error = "File exceeds the maximum allowed size of 5 MB." });
        }

        if (!AllowedContentTypes.Contains(file.ContentType))
        {
            return BadRequest(new { error = "Only PNG, JPEG, GIF or WEBP images are allowed." });
        }

        using Stream stream = file.OpenReadStream();
        SavedImageResult saved = await _fileStorageService.SaveImageWithDimensionsAsync(stream, file.FileName, file.ContentType, width, cancellationToken);

        if (form.FestivalId.HasValue && form.FestivalId.Value != Guid.Empty)
        {
            MediaAsset mediaAsset = new()
            {
                FestivalId = form.FestivalId.Value,
                Url = saved.Url,
                FileName = file.FileName,
                Width = saved.Width,
                Height = saved.Height
            };

            await _mediaAssetRepository.AddAsync(mediaAsset, cancellationToken);
            await _mediaAssetRepository.SaveChangesAsync(cancellationToken);
        }

        return Ok(new { url = saved.Url, width = saved.Width, height = saved.Height });
    }

    [HttpGet("gallery")]
    public async Task<IActionResult> GetGallery([FromQuery] Guid festivalId, CancellationToken cancellationToken)
    {
        IReadOnlyList<MediaAsset> assets = await _mediaAssetRepository.GetByFestivalIdAsync(festivalId, cancellationToken);

        return Ok(assets.Select(a => new
        {
            id = a.Id,
            url = a.Url,
            width = a.Width,
            height = a.Height,
            createdAt = a.CreatedAt
        }));
    }
}