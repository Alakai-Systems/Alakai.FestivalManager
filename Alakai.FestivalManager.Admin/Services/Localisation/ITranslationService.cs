namespace Alakai.FestivalManager.Admin.Services.Localisation;

public interface ITranslationService
{
    string Language { get; }
    string Get(string key, string? fallback = null);
    Task SetLanguageAsync(string language);
    Task DetectAndSetLanguageAsync();
}