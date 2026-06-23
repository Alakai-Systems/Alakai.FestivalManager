using Alakai.FestivalManager.Admin.Services.Auth;

namespace Alakai.FestivalManager.Application.Extensions;

public static class ApiCLientsDependencyInjectionExtension
{
    public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<FestivalApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<EditionApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<PassTypeApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<LevelApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<RegistrationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<CompetitionEntryApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<CompetitionApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<DiscountCodeApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<EmailLogApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<EmailTemplateApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<UserApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<UserPanelApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddScoped<ITokenStorageService,
            TokenStorageService>();

        services.AddScoped<ProtectedLocalStorage>();

        return services;
    }
}
