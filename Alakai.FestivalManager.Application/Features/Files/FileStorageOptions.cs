namespace Alakai.FestivalManager.Infrastructure.Files;

public class FileStorageOptions
{
    /// <summary>Physical folder, relative to the API's content root, where files are saved.</summary>
    public string RootPath { get; set; } = "wwwroot/uploads/email-images";

    /// <summary>Public base URL clients use to fetch the saved files.</summary>
    public string PublicBaseUrl { get; set; } = string.Empty;
}