namespace ETAM.Infrastructure.Services;

/// <summary>Paramètres SMTP liés depuis la section "Smtp" de appsettings.json.</summary>
public class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
    public string FromEmail { get; set; } = "no-reply@etam.mg";
    public string FromName { get; set; } = "ETAM ERP";
    /// <summary>Si vrai, l'email est écrit dans les logs au lieu d'être envoyé (dev).</summary>
    public bool Simulation { get; set; } = true;
}
