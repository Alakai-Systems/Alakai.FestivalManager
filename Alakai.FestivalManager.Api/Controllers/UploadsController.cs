using Alakai.FestivalManager.Application.Features.Files.Services;

namespace Alakai.FestivalManager.Api.Controllers;

[ApiController]
[Route("api/uploads")]
public class UploadsController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/gif", "image/webp"
    };

    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadsController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }
    [HttpPost("images")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> UploadImage([FromForm] IFormFile file, [FromForm] int? width, CancellationToken cancellationToken)
    {
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
        string url = await _fileStorageService.SaveImageAsync(stream, file.FileName, file.ContentType, width, cancellationToken);

        return Ok(new { url });
    }
}