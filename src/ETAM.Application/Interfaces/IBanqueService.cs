using ETAM.Application.Common.Models;
using ETAM.Domain.Enums;

namespace ETAM.Application.Interfaces;

/// <summary>Gestion de la trésorerie : mouvements bancaires et paiement des dettes.</summary>
public interface IBanqueService
{
    /// <summary>
    /// Enregistre un mouvement bancaire (dépôt, retrait, virement, frais) et met à jour
    /// le solde du compte. Un débit supérieur au solde est refusé.
    /// </summary>
    Task<Result> EnregistrerMouvementAsync(
        long compteId, TypeMouvementBancaire type, decimal montant,
        string? beneficiaire, string? motif, string? reference, long? chantierId,
        CancellationToken ct = default);

    /// <summary>
    /// Paie (tout ou partie) une dette fournisseur par virement bancaire :
    /// débite le compte, réduit la dette, met à jour son statut, et — en option —
    /// crée la dépense correspondante imputée au Budget Comptes.
    /// </summary>
    Task<Result> PayerDetteAsync(
        long detteId, long compteId, decimal montant, bool genererDepense,
        CancellationToken ct = default);
}
