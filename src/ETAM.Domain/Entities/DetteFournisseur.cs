using ETAM.Domain.Common;
using ETAM.Domain.Enums;

namespace ETAM.Domain.Entities;

/// <summary>
/// Dette envers un fournisseur (ex : dette ciment, dette fer).
/// Réglée par virements bancaires successifs (paiements partiels possibles).
/// </summary>
public class DetteFournisseur : BaseEntity
{
    public long FournisseurId { get; set; }
    public Fournisseur Fournisseur { get; set; } = null!;

    public long? ChantierId { get; set; }
    public Chantier? Chantier { get; set; }

    public string Libelle { get; set; } = null!;
    public decimal MontantInitial { get; set; }
    public decimal MontantPaye { get; set; }
    public DateTime? DateEcheance { get; set; }

    public StatutDette Statut { get; set; } = StatutDette.Ouverte;

    // --- Propriétés calculées ---
    public decimal SoldeRestant => MontantInitial - MontantPaye;
    public double PourcentagePaye =>
        MontantInitial <= 0 ? 0 : (double)(MontantPaye / MontantInitial) * 100d;
}
