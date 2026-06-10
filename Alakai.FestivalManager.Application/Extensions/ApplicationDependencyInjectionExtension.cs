namespace Alakai.FestivalManager.Application.Extensions;

public static class ApplicationDependencyInjectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {

        services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjectionExtension).Assembly);
        services.AddAutoMapper(cfg =>
        {
            cfg.AddMaps(typeof(ApplicationDependencyInjectionExtension).Assembly);
        });

        services.AddScoped<CreateFestivalHandler>();
        services.AddScoped<IFestivalService, FestivalService>();
        services.AddScoped<GetFestivalByIdHandler>();
        services.AddScoped<GetFestivalsHandler>();
        services.AddScoped<UpdateFestivalHandler>();

        return services;
    }
}
