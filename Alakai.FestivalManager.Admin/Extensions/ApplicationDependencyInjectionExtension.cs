namespace Alakai.FestivalManager.Application.Extensions;

public static class ApiCLientsDependencyInjectionExtension
{
    public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<AdminAuthDelegatingHandler>();

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
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<EditionApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<PassTypeApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<ProductionAccommodationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<RunnerItineraryApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<ProductionTripApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<ProductionBuildingApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<ProductionZoneApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<ProductionReservationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<ProductionPersonApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<ProductionSupplierApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<LevelApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<RegistrationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<CompetitionEntryApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<CompetitionApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<DiscountCodeApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();


        services.AddHttpClient<DashboardApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<AnalyticsApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<EmailLogApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<EmailTemplateApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<UploadsApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<EmailLayoutApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<UserApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
            // El import masivo de CSV puede tardar mas que el timeout por defecto de
            // HttpClient (100s) con miles de filas. El resto de llamadas de este cliente
            // (un solo usuario) siguen respondiendo en milisegundos -- esto solo sube el
            // limite maximo de espera.
            client.Timeout = TimeSpan.FromMinutes(10);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<InvoiceApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<Alakai.FestivalManager.Admin.Services.Api.ImpersonationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<InvoiceSettingsApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

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
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<MealPreferenceApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

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
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<AccommodationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<InvoiceTemplateApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<EmailNotificationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

        services.AddHttpClient<TicketsApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminAuthDelegatingHandler>();

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
