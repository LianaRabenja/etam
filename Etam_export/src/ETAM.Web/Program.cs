using ETAM.Application;
using ETAM.Infrastructure;
using ETAM.Infrastructure.Identity;
using ETAM.Infrastructure.Persistence;
using ETAM.Infrastructure.Persistence.Seed;
using ETAM.Web.Middleware;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Serilog;

// Licence QuestPDF (gratuite pour usage Community / petites entreprises).
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// ---------- Serilog ----------
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/etam-.log", rollingInterval: RollingInterval.Day));

// ---------- Couches ----------
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// ---------- MVC + FluentValidation ----------
builder.Services.AddControllersWithViews();
builder.Services.AddFluentValidationAutoValidation()
                .AddFluentValidationClientsideAdapters();

// ---------- Compression des réponses (HTML/CSS/JS/JSON) : réduit la taille transférée
// sans changer le contenu ni le comportement des pages. ----------
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);
builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);

// ---------- Cookies d'authentification ----------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ---------- Hangfire (tâches planifiées : évaluation des alertes) ----------
var hangfireConn = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(hangfireConn))
{
    builder.Services.AddHangfire(cfg =>
        cfg.UsePostgreSqlStorage(c => c.UseNpgsqlConnection(hangfireConn)));
    builder.Services.AddHangfireServer();
}

var app = builder.Build();

// ---------- Pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    // Le fichier local etam.css est déjà versionné (asp-append-version="true"), donc un cache
    // long (1 an) est sûr : toute modification du fichier change son URL, donc invalide le cache.
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
    }
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---------- Migration + Seed au démarrage ----------
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    try
    {
        var context = sp.GetRequiredService<ApplicationDbContext>();
        var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = sp.GetRequiredService<ILogger<Program>>();
        await DbInitializer.SeedAsync(context, userManager, roleManager, logger);
    }
    catch (Exception ex)
    {
        var logger = sp.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erreur lors de l'initialisation de la base de données.");
    }
}

app.Run();
