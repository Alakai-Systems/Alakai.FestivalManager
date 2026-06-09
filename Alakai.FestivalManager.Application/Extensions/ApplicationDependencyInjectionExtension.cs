namespace Alakai.FestivalManager.Application.Extensions;

public static class ApplicationDependencyInjectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<CreateFestivalHandler>();

        services.AddScoped<IFestivalService, FestivalService>();

        services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjectionExtension).Assembly);

        return services;
    }
}
