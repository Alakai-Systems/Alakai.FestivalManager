namespace Alakai.FestivalManager.Admin.Services.Api;

public class ApiClientException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ApiClientException(string message, IReadOnlyList<string>? errors = null) : base(message)
    {
        Errors = errors ?? [];
    }
}
