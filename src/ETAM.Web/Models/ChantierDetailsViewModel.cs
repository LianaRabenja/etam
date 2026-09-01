using ETAM.Domain.Entities;

namespace ETAM.Web.Models;

/// <summary>Vue 360° d'un chantier : tout ce qui s'y rapporte.</summary>
public class ChantierDetailsViewModel
{
    public Chantier Chantier { get; set; } = null!;
    public CompteBancaire? CompteBancaire { get; set; }
    public List<MouvementBancaire> Mouvements { get; set; } = new();
    public List<Materiau> Materiaux { get; set; } = new();
    public List<Approvisionnement> Approvisionnements { get; set; } = new();
    public List<PrevisionJournaliere> Previsions { get; set; } = new();
    public List<Depense> Depenses { get; set; } = new();
    public List<DetteFournisseur> Dettes { get; set; } = new();
    public List<Alerte> Alertes { get; set; } = new();
    public List<RapportTravail> RapportsTravail { get; set; } = new();

    /// <summary>
    /// Total sorti de la banque pour ce chantier depuis le début : somme des retraits
    /// déclenchés par l'exécution des prévisions journalières.
    /// </summary>
    public decimal TotalRetire { get; set; }

    /// <summary>
    /// Argent retiré mais pas encore distribué — ce que le chef de chantier détient
    /// physiquement. Sans cet indicateur, cette somme n'apparaissait nulle part :
    /// elle n'existait que dans le report du lendemain.
    /// </summary>
    public decimal CaisseChantier => TotalRetire - Chantier.Consommation;
}
