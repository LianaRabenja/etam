using System.Reflection;
using ETAM.Application.Interfaces;
using ETAM.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ETAM.Application;

/// <summary>Enregistrement des services de la couche Application.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IPrevisionService, PrevisionService>();
        services.AddScoped<IBanqueService, BanqueService>();

        return services;
    }
}
