namespace Alakai.FestivalManager.Application.Features.Files.Services;

public interface IFileStorageService
{
    /// <param name="targetWidth">If provided and smaller than the original image width, the image is resized (proportionally) to this width before saving. Never upscales.</param>
    Task<string> SaveImageAsync(Stream content, string fileName, string contentType, int? targetWidth = null, CancellationToken cancellationToken = default);

    /// <summary>Saves a non-image file (e.g. a generated PDF) as-is, without any image processing.</summary>
    Task<string> SaveFileAsync(Stream content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>Resolves a previously-returned public URL back to its physical file path on disk, or null if it doesn't match this storage's public base URL.</summary>
    string? ResolveLocalPath(string publicUrl);
}