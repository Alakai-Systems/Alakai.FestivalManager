using Alakai.FestivalManager.Application.Features.Editions.Queries.GetEditionsByFestivalId;
using Alakai.FestivalManager.Application.Features.PassTypes.Services;

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

        //Festivals
        services.AddScoped<CreateFestivalHandler>();
        services.AddScoped<IFestivalService, FestivalService>();
        services.AddScoped<GetFestivalByIdHandler>();
        services.AddScoped<GetFestivalsHandler>();
        services.AddScoped<UpdateFestivalHandler>();
        services.AddScoped<DeleteFestivalHandler>();

        //Editions
        services.AddScoped<CreateEditionHandler>();
        services.AddScoped<GetEditionByIdHandler>();
        services.AddScoped<GetEditionsByFestivalIdHandler>();
        services.AddScoped<GetEditionsHandler>();
        services.AddScoped<UpdateEditionHandler>();
        services.AddScoped<DeleteEditionHandler>();
        services.AddScoped<IEditionService, EditionService>();

        //PassTypes
        services.AddScoped<CreatePassTypeHandler>();
        services.AddScoped<GetPassTypeByIdHandler>();
        services.AddScoped<GetPassTypesHandler>();
        services.AddScoped<GetPassTypesByEditionIdHandler>();
        services.AddScoped<UpdatePassTypeHandler>();
        services.AddScoped<DeletePassTypeHandler>();
        services.AddScoped<IPassTypeService, PassTypeService>();

        return services;
    }
}
