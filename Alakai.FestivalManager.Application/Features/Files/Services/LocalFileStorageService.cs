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

    public string? ResolveLocalPath(string publicUrl)
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