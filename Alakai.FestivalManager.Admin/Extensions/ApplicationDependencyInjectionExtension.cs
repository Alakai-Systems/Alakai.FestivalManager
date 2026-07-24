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
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<FestivalApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<EditionApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<PassTypeApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<ProductionAccommodationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<RunnerItineraryApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<ProductionTripApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<ProductionBuildingApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<ProductionZoneApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<ProductionReservationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<ProductionPersonApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<ProductionSupplierApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<LevelApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<RegistrationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<CompetitionEntryApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<CompetitionApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<DiscountCodeApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();


        services.AddHttpClient<DashboardApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<AnalyticsApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<EmailLogApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<EmailTemplateApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<UploadsApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<EmailLayoutApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<UserApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<InvoiceApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<Alakai.FestivalManager.Admin.Services.Api.ImpersonationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<InvoiceSettingsApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<UserPanelApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<BusApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<MealPreferenceApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<PaymentApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<ReportApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<AccommodationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<InvoiceTemplateApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddHttpClient<EmailNotificationApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();

        services.AddScoped<ITokenStorageService, TokenStorageService>();

        services.AddHttpClient<FestivalModuleApiClient>(client =>
        {
            string baseUrl = configuration["ApiSettings:BaseUrl"]
                ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");
            client.BaseAddress = new Uri(baseUrl);
        }).AddHttpMessageHandler<AdminBearerTokenHandler>();
        services.AddScoped<ActiveFestivalState>();
        services.AddScoped<ITranslationService, TranslationService>();

        services.AddScoped<UserProfileState>();

        services.AddScoped<IAdminTokenProvider, AdminTokenProvider>();
        services.AddTransient<AdminBearerTokenHandler>();

        services.AddScoped<ProtectedLocalStorage>();

        return services;
    }
}
