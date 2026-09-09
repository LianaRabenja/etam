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
}
