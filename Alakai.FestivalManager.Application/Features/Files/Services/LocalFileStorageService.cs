using Alakai.FestivalManager.Application.Features.Files.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Alakai.FestivalManager.Infrastructure.Files;

public class LocalFileStorageService : IFileStorageService
{
    private readonly FileStorageOptions _options;

    public LocalFileStorageService(IOptions<FileStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> SaveImageAsync(Stream content, string fileName, string contentType, int? targetWidth = null, CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => ".bin"
            };
        }

        string uniqueFileName = $"{Guid.NewGuid()}{extension}";
        string physicalFolder = Path.Combine(Directory.GetCurrentDirectory(), _options.RootPath);

        Directory.CreateDirectory(physicalFolder);

        string physicalPath = Path.Combine(physicalFolder, uniqueFileName);

        using (SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(content, cancellationToken))
        {
            if (targetWidth.HasValue && targetWidth.Value > 0 && targetWidth.Value < image.Width)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(targetWidth.Value, 0)
                }));
            }

            await image.SaveAsync(physicalPath, cancellationToken);
        }

        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{uniqueFileName}";
    }

    public async Task<SavedImageResult> SaveImageWithDimensionsAsync(Stream content, string fileName, string contentType, int? targetWidth = null, CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(fileName);

        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = contentType switch
            {
                "image/png" => ".png",
                "image/jpeg" => ".jpg",
                "image/gif" => ".gif",
                "image/webp" => ".webp",
                _ => ".bin"
            };
        }

        string uniqueFileName = $"{Guid.NewGuid()}{extension}";
        string physicalFolder = Path.Combine(Directory.GetCurrentDirectory(), _options.RootPath);

        Directory.CreateDirectory(physicalFolder);

        string physicalPath = Path.Combine(physicalFolder, uniqueFileName);

        using SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(content, cancellationToken);

        if (targetWidth.HasValue && targetWidth.Value > 0 && targetWidth.Value < image.Width)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(targetWidth.Value, 0)
            }));
        }

        await image.SaveAsync(physicalPath, cancellationToken);

        string url = $"{_options.PublicBaseUrl.TrimEnd('/')}/{uniqueFileName}";

        return new SavedImageResult(url, image.Width, image.Height);
    }

    public async Task<string> SaveFileAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(fileName);
        string uniqueFileName = $"{Guid.NewGuid()}{extension}";
        string physicalFolder = Path.Combine(Directory.GetCurrentDirectory(), _options.RootPath);

        Directory.CreateDirectory(physicalFolder);

        string physicalPath = Path.Combine(physicalFolder, uniqueFileName);

        using (FileStream fileStream = new(physicalPath, FileMode.Create, FileAccess.Write))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
        }

        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{uniqueFileName}";
    }

    public async Task<byte[]?> TryDownloadAsync(string publicUrl, CancellationToken cancellationToken = default)
    {
        string? physicalPath = ResolveLocalPath(publicUrl);

        if (physicalPath is null || !File.Exists(physicalPath))
        {
            return null;
        }

        return await File.ReadAllBytesAsync(physicalPath, cancellationToken);
    }

    public Task DeleteAsync(string? publicUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return Task.CompletedTask;
        }

        string? physicalPath = ResolveLocalPath(publicUrl);

        try
        {
            if (physicalPath is not null && File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }
        }
        catch (IOException)
        {
            // Mejor esfuerzo: fichero bloqueado o problema de disco - no debe impedir
            // que se complete el borrado del registro.
        }
        catch (UnauthorizedAccessException)
        {
            // Igual que arriba - problema de permisos.
        }

        return Task.CompletedTask;
    }

    private string? ResolveLocalPath(string publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return null;
        }

        string prefix = _options.PublicBaseUrl.TrimEnd('/') + "/";

        if (!publicUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string fileName = publicUrl[prefix.Length..];
        string physicalFolder = Path.Combine(Directory.GetCurrentDirectory(), _options.RootPath);

        return Path.Combine(physicalFolder, fileName);
    }
}