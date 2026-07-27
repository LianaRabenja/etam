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
    /// <summary>
    /// Accepte aussi bien le format Npgsql classique (« Host=...;Username=... ») que l'URI
    /// fournie par les hébergeurs comme Render (« postgresql://user:mdp@hote:port/base »),
    /// et force le SSL en dehors de localhost.
    /// </summary>
    private static string NormaliserChaineConnexion(string valeur)
    {
        valeur = valeur.Trim();

        if (!valeur.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !valeur.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
            return valeur;

        var uri = new Uri(valeur);
        var infos = uri.UserInfo.Split(':', 2);
        var utilisateur = Uri.UnescapeDataString(infos[0]);
        var motDePasse = infos.Length > 1 ? Uri.UnescapeDataString(infos[1]) : string.Empty;
        var base_ = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;
        var local = uri.Host is "localhost" or "127.0.0.1";

        return $"Host={uri.Host};Port={port};Database={base_};" +
               $"Username={utilisateur};Password={motDePasse};" +
               (local ? "SSL Mode=Disable" : "SSL Mode=Require;Trust Server Certificate=true");
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Ordre de priorité : variable d'environnement ETAM_CONNECTION (production / Render),
        // puis la configuration (appsettings.Development.json en local).
        // Aucun mot de passe n'est codé en dur : l'application refuse de démarrer sans configuration.
        var connectionString = Environment.GetEnvironmentVariable("ETAM_CONNECTION");
        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Chaîne de connexion absente. Définissez la variable d'environnement ETAM_CONNECTION " +
                "ou ConnectionStrings:DefaultConnection dans la configuration.");

        connectionString = NormaliserChaineConnexion(connectionString);

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
