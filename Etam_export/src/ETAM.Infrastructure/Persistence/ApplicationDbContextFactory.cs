using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ETAM.Application.Interfaces;
using ETAM.Domain.Enums;
using ETAM.Infrastructure.Persistence.Interceptors;

namespace ETAM.Infrastructure.Persistence;

/// <summary>
/// Fabrique design-time utilisée par les outils EF Core (dotnet ef migrations add ...).
/// Utilise une chaîne de connexion par défaut surchargée par la variable
/// d'environnement ETAM_CONNECTION si présente.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var conn = Environment.GetEnvironmentVariable("ETAM_CONNECTION")
                   ?? "Host=localhost;Port=5432;Database=etam_erp;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(conn, o => o.MigrationsAssembly("ETAM.Infrastructure"))
            .Options;

        return new ApplicationDbContext(options, new AuditableEntityInterceptor(new NoOpUser()));
    }

    /// <summary>Utilisateur factice pour le design-time (aucun contexte HTTP).</summary>
    private sealed class NoOpUser : ICurrentUserService
    {
        public string? UserId => "design-time";
        public string? UserName => "design-time";
        public string? IpAddress => null;
        public string? UserAgent => null;
        public bool IsInRole(string role) => false;
    }
}
