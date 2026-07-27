using ETAM.Application.Interfaces;
using ETAM.Domain.Interfaces;
using ETAM.Infrastructure.Identity;
using ETAM.Infrastructure.Persistence;
using ETAM.Infrastructure.Persistence.Interceptors;
using ETAM.Infrastructure.Persistence.Repositories;
using ETAM.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ETAM.Infrastructure;

/// <summary>Enregistrement de la persistance, de l'identité et des services d'infrastructure.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=etam_erp;Username=postgres;Password=postgres";

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npg => npg.MigrationsAssembly("ETAM.Infrastructure")));

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedAccount = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        // Repository Pattern + Unit Of Work
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services d'infrastructure
        services.AddMemoryCache();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IAlerteService, AlerteService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReferenceDataCache, ReferenceDataCache>();

        // Email SMTP
        services.Configure<Services.SmtpOptions>(config.GetSection("Smtp"));
        services.AddScoped<IEmailSender, Services.SmtpEmailSender>();

        return services;
    }
}
