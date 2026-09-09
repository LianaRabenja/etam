using ETAM.Application.Common.Models;
using ETAM.Domain.Enums;

namespace ETAM.Application.Interfaces;

/// <summary>Paramètres d'un décaissement réel, au fil de l'eau.</summary>
public record DecaissementDto
{
    public long PrevisionJournaliereId { get; init; }
    public long? PrevisionLigneId { get; init; }
    public DateTime Date { get; init; } = DateTime.UtcNow;
    public string Beneficiaire { get; init; } = string.Empty;
    public string Motif { get; init; } = string.Empty;
    public decimal Montant { get; init; }
    public ModePaiement Mode { get; init; } = ModePaiement.Especes;
    public long CompteBancaireId { get; init; }
    public TypeBudget BudgetConcerne { get; init; } = TypeBudget.Materiel;
    public string? Reference { get; init; }
    public string? AccuseNom { get; init; }
    public string? Observation { get; init; }
}

/// <summary>
/// Sorties d'argent réelles à l'intérieur d'une prévision journalière.
/// C'est le seul point du logiciel qui débite un compte bancaire.
/// </summary>
public interface IDecaissementService
{
    /// <summary>
    /// Enregistre un paiement : débite la banque, augmente la consommation du chantier
    /// ou du Budget Comptes, diminue le stock et la dette le cas échéant, et réduit
    /// d'autant le reliquat du jour et de l'enveloppe mensuelle.
    /// </summary>
    Task<Result<long>> EnregistrerAsync(DecaissementDto dto, CancellationToken ct = default);

    /// <summary>Annule un décaissement et remet tous les compteurs en état.</summary>
    Task<Result> AnnulerAsync(long id, string motif, CancellationToken ct = default);
}
