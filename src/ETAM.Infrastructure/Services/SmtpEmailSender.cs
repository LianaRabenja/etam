using System.Net;
using System.Net.Mail;
using ETAM.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ETAM.Infrastructure.Services;

/// <summary>
/// Implémentation SMTP de l'envoi d'emails. En mode Simulation (par défaut),
/// l'email est journalisé au lieu d'être réellement envoyé — pratique tant
/// qu'aucun serveur SMTP n'est configuré.
/// </summary>
public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml, CancellationToken ct = default)
    {
        if (_options.Simulation || string.IsNullOrWhiteSpace(_options.Host))
        {
            _logger.LogInformation("EMAIL (simulation) -> {To} | {Sujet}\n{Corps}", destinataire, sujet, corpsHtml);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromEmail, _options.FromName),
            Subject = sujet,
            Body = corpsHtml,
            IsBodyHtml = true
        };
        message.To.Add(destinataire);

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            Credentials = new NetworkCredential(_options.User, _options.Password)
        };

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Email envoyé à {To} ({Sujet}).", destinataire, sujet);
    }
}
