namespace Alakai.FestivalManager.Application.Features.Files.Services;

public interface IFileStorageService
{
    /// <param name="targetWidth">If provided and smaller than the original image width, the image is resized (proportionally) to this width before saving. Never upscales.</param>
    Task<string> SaveImageAsync(Stream content, string fileName, string contentType, int? targetWidth = null, CancellationToken cancellationToken = default);
}