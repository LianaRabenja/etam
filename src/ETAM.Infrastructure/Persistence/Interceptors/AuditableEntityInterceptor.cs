using ETAM.Application.Interfaces;
using ETAM.Domain.Common;
using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ETAM.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Renseigne automatiquement CreatedAt/UpdatedAt/CreatedBy/UpdatedBy, convertit les
/// suppressions en soft-delete (IsDeleted) et écrit une entrée dans le journal d'audit
/// pour chaque Ajout / Modification / Suppression fait par un utilisateur connecté.
/// </summary>
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public AuditableEntityInterceptor(ICurrentUserService currentUser) => _currentUser = currentUser;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Apply(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        Apply(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void Apply(DbContext? context)
    {
        if (context is null) return;
        var user = _currentUser.UserName ?? _currentUser.UserId ?? "système";
        var now = DateTime.UtcNow;

        // On n'audite que les actions d'un utilisateur réellement connecté
        // (pas le seed / les tâches système), pour garder le journal lisible.
        bool auditerActions = !string.IsNullOrEmpty(_currentUser.UserId) || !string.IsNullOrEmpty(_currentUser.UserName);

        var journaux = new List<AuditLog>();

        foreach (EntityEntry<BaseEntity> entry in context.ChangeTracker.Entries<BaseEntity>().ToList())
        {
            // Ne jamais auditer le journal lui-même (évite la récursion).
            if (entry.Entity is AuditLog) continue;

            TypeActionAudit? action = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = user;
                    action = TypeActionAudit.Ajout;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = user;
                    action = entry.Entity.IsDeleted ? TypeActionAudit.Suppression : TypeActionAudit.Modification;
                    break;

                case EntityState.Deleted:
                    // Soft delete : on n'efface jamais physiquement.
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = user;
                    action = TypeActionAudit.Suppression;
                    break;
            }

            if (auditerActions && action is not null)
            {
                journaux.Add(new AuditLog
                {
                    Action = action.Value,
                    Entite = LibelleEntite(entry.Entity.GetType().Name),
                    CleEntite = entry.Entity.Id != 0 ? entry.Entity.Id.ToString() : null,
                    NouvelleValeur = DescriptionEntite(entry.Entity),
                    UtilisateurId = _currentUser.UserId,
                    UtilisateurNom = user,
                    AdresseIp = _currentUser.IpAddress,
                    Navigateur = _currentUser.UserAgent,
                    DateAction = now,
                    CreatedAt = now,
                    CreatedBy = user
                });
            }
        }

        if (journaux.Count > 0)
            context.AddRange(journaux);
    }

    // Nom lisible de l'entité (ex : "MouvementMateriau" -> "Mouvement matériau").
    private static string LibelleEntite(string typeName) => typeName switch
    {
        "Materiau" => "Matériau",
        "MouvementMateriau" => "Mouvement matériau",
        "Chantier" => "Chantier",
        "Depense" => "Dépense",
        "PrevisionJournaliere" => "Prévision",
        "RapportTravail" => "Rapport de travail",
        "Approvisionnement" => "Approvisionnement",
        "Fournisseur" => "Fournisseur",
        "DetteFournisseur" => "Dette fournisseur",
        "CompteBancaire" => "Compte bancaire",
        "MouvementBancaire" => "Mouvement bancaire",
        "BudgetCompte" => "Budget",
        "ArticleCatalogue" => "Article catalogue",
        "Parametre" => "Paramètre",
        _ => typeName
    };

    // Essaie d'extraire un libellé parlant (désignation, nom, référence...) pour le journal.
    private static string? DescriptionEntite(object e)
    {
        var type = e.GetType();
        foreach (var nom in new[] { "Designation", "Nom", "Libelle", "Reference", "Numero", "Titre", "Motif", "Cle" })
        {
            var prop = type.GetProperty(nom);
            if (prop?.GetValue(e) is string valeur && !string.IsNullOrWhiteSpace(valeur))
                return valeur.Length > 200 ? valeur[..200] : valeur;
        }
        return null;
    }
}
