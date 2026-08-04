namespace Alakai.FestivalManager.Application.Features.Files.Services;

public interface IFileStorageService
{
    /// <param name="targetWidth">If provided and smaller than the original image width, the image is resized (proportionally) to this width before saving. Never upscales.</param>
    Task<string> SaveImageAsync(Stream content, string fileName, string contentType, int? targetWidth = null, CancellationToken cancellationToken = default);

    /// <summary>Same as SaveImageAsync, but also returns the final width/height after any resize - for callers that need to record the real dimensions (e.g. the media gallery).</summary>
    Task<SavedImageResult> SaveImageWithDimensionsAsync(Stream content, string fileName, string contentType, int? targetWidth = null, CancellationToken cancellationToken = default);

    /// <summary>Saves a non-image file (e.g. a generated PDF) as-is, without any image processing.</summary>
    Task<string> SaveFileAsync(Stream content, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the bytes of a previously-saved file from its public URL, or null if the
    /// URL doesn't belong to this storage or the file no longer exists (e.g. wiped by a
    /// deploy when using local disk storage). Works the same way regardless of the actual
    /// storage backend (local disk, Azure Blob Storage, etc).
    /// </summary>
    Task<byte[]?> TryDownloadAsync(string publicUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un fichero previamente guardado a partir de su URL publica. Es "mejor
    /// esfuerzo": si la URL esta vacia, no pertenece a este storage, el fichero ya no
    /// existe, o falla el borrado (red, permisos), no lanza excepcion - nunca debe romper
    /// el flujo que lo llama (p.ej. borrar un registro y sus PDFs asociados).
    /// </summary>
    Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default);
}

public record SavedImageResult(string Url, int Width, int Height);