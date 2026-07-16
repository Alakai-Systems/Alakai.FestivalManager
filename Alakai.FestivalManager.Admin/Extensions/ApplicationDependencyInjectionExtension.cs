namespace Alakai.FestivalManager.Application.Extensions;

public static class ApiCLientsDependencyInjectionExtension
{
    public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<PublicRegistrationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

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


        services.AddHttpClient<DashboardApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<AnalyticsApiClient>(client =>
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

        services.AddHttpClient<UploadsApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<EmailLayoutApiClient>(client =>
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

        services.AddHttpClient<InvoiceApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<InvoiceSettingsApiClient>(client =>
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

        services.AddHttpClient<BusApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<MealPreferenceApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<PaymentApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<ReportApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<AccommodationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<InvoiceTemplateApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<EmailNotificationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddScoped<ITokenStorageService, TokenStorageService>();

        services.AddHttpClient<FestivalModuleApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        });
        services.AddScoped<ActiveFestivalState>();
        services.AddScoped<ITranslationService, TranslationService>();

        services.AddScoped<UserProfileState>();

        services.AddScoped<IAdminTokenProvider, AdminTokenProvider>();

        services.AddScoped<ProtectedLocalStorage>();

        return services;
    }
}
