namespace ETAM.Domain.Enums;

/// <summary>
/// Workflow du rapport de travail (rapport hebdomadaire d'avancement) :
/// Brouillon -> Soumis (par le Correspondant) -> Validé (par l'Administrateur) ou Refusé.
/// </summary>
public enum StatutRapportTravail
{
    Brouillon = 0,
    Soumis = 1,
    Valide = 2,
    Refuse = 3
}
