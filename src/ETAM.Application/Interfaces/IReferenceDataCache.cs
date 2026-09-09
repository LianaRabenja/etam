using ETAM.Domain.Entities;

namespace ETAM.Application.Interfaces;

/// <summary>
/// Cache très court (quelques dizaines de secondes) pour les listes de référence rechargées
/// en permanence dans les menus déroulants (ex : liste des chantiers). Objectif : éviter de
/// refaire la même requête à chaque affichage de formulaire (Créer/Modifier), sans changer
/// aucune logique métier — juste un rafraîchissement légèrement différé (quelques secondes).
/// </summary>
public interface IReferenceDataCache
{
    Task<IReadOnlyList<Chantier>> ObtenirChantiersAsync(CancellationToken ct = default);

    /// <summary>À appeler après création/modification d'un chantier pour forcer un rafraîchissement immédiat.</summary>
    void InvaliderChantiers();
}
