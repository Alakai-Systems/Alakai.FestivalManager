using System.Text.Json;
using Microsoft.JSInterop;

namespace Alakai.FestivalManager.Admin.Services.Localisation;

public class TranslationService : ITranslationService
{
    private readonly IJSRuntime _js;
    private readonly IWebHostEnvironment _env;
    private Dictionary<string, string> _translations = [];
    private static readonly string[] SupportedLanguages = ["en", "es", "fr", "ca"];

    public string Language { get; private set; } = "en";

    public TranslationService(IJSRuntime js, IWebHostEnvironment env)
    {
        _js = js;
        _env = env;
    }

    public string Get(string key, string? fallback = null)
    {
        if (_translations.TryGetValue(key, out string? value))
        {
            return value;
        }

        return fallback ?? key;
    }

    public async Task SetLanguageAsync(string language)
    {
        string lang = SupportedLanguages.Contains(language) ? language : "en";
        Language = lang;
        await LoadTranslationsAsync(lang);
    }

    public async Task DetectAndSetLanguageAsync()
    {
        try
        {
            string browserLang = await _js.InvokeAsync<string>("navigator.language") ?? "en";
            string lang = browserLang.Length >= 2 ? browserLang[..2].ToLower() : "en";
            await SetLanguageAsync(lang);
        }
        catch
        {
            await SetLanguageAsync("en");
        }
    }

    private async Task LoadTranslationsAsync(string language)
    {
        try
        {
            string path = Path.Combine(_env.WebRootPath, "i18n", $"{language}.json");

            if (!File.Exists(path))
            {
                path = Path.Combine(_env.WebRootPath, "i18n", "en.json");
            }

            string json = await File.ReadAllTextAsync(path);
            _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
        }
        catch
        {
            _translations = [];
        }
    }
}