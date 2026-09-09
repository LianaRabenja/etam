namespace ETAM.Application.Interfaces;

/// <summary>Envoi d'emails (réinitialisation de mot de passe, notifications...).</summary>
public interface IEmailSender
{
    Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml, CancellationToken ct = default);
}
