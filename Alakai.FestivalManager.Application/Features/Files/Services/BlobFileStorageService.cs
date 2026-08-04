using Alakai.FestivalManager.Application.Features.Files.Services;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Alakai.FestivalManager.Infrastructure.Files;

public class BlobFileStorageService : IFileStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobFileStorageService(BlobServiceClient blobServiceClient, IOptions<AzureBlobStorageOptions> options)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient(options.Value.ContainerName);
    }

    public async Task<string> SaveImageAsync(Stream content, string fileName, string contentType, int? targetWidth = null, CancellationToken cancellationToken = default)
    {
        (MemoryStream processedStream, string extension, int _, int _) = await ProcessImageAsync(content, fileName, contentType, targetWidth, cancellationToken);

        using (processedStream)
        {
            string blobName = $"{Guid.NewGuid()}{extension}";
            return await UploadAsync(processedStream, blobName, contentType, cancellationToken);
        }
    }

    public async Task<SavedImageResult> SaveImageWithDimensionsAsync(Stream content, string fileName, string contentType, int? targetWidth = null, CancellationToken cancellationToken = default)
    {
        (MemoryStream processedStream, string extension, int width, int height) = await ProcessImageAsync(content, fileName, contentType, targetWidth, cancellationToken);

        using (processedStream)
        {
            string blobName = $"{Guid.NewGuid()}{extension}";
            string url = await UploadAsync(processedStream, blobName, contentType, cancellationToken);
            return new SavedImageResult(url, width, height);
        }
    }

    public async Task<string> SaveFileAsync(Stream content, string fileName, CancellationToken cancellationToken = default)
    {
        string extension = Path.GetExtension(fileName);
        string blobName = $"{Guid.NewGuid()}{extension}";
        string contentType = GetContentType(extension);

        return await UploadAsync(content, blobName, contentType, cancellationToken);
    }

    public async Task<byte[]?> TryDownloadAsync(string publicUrl, CancellationToken cancellationToken = default)
    {
        string? blobName = TryGetBlobName(publicUrl);

        if (blobName is null)
        {
            return null;
        }

        try
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobName);
            using MemoryStream memoryStream = new();
            await blobClient.DownloadToAsync(memoryStream, cancellationToken);
            return memoryStream.ToArray();
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    private async Task<string> UploadAsync(Stream content, string blobName, string contentType, CancellationToken cancellationToken)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob, cancellationToken: cancellationToken);

        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        BlobUploadOptions uploadOptions = new()
        {
            HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
        };

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

        return blobClient.Uri.ToString();
    }

    private async Task<(MemoryStream Stream, string Extension, int Width, int Height)> ProcessImageAsync(Stream content, string fileName, string contentType, int? targetWidth, CancellationToken cancellationToken)
    {
        using SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(content, cancellationToken);

        if (targetWidth.HasValue && targetWidth.Value > 0 && targetWidth.Value < image.Width)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(targetWidth.Value, 0)
            }));
        }

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

        MemoryStream memoryStream = new();
        SixLabors.ImageSharp.Formats.IImageFormat format = image.Metadata.DecodedImageFormat ?? SixLabors.ImageSharp.Formats.Png.PngFormat.Instance;
        await image.SaveAsync(memoryStream, format, cancellationToken);
        memoryStream.Position = 0;

        return (memoryStream, extension, image.Width, image.Height);
    }

    private string? TryGetBlobName(string publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
        {
            return null;
        }

        string prefix = _containerClient.Uri.ToString().TrimEnd('/') + "/";

        if (!publicUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return publicUrl[prefix.Length..];
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".pdf" => "application/pdf",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}