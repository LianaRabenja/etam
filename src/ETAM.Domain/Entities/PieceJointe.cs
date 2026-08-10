using ETAM.Domain.Common;

namespace ETAM.Domain.Entities;

/// <summary>
/// Justificatif numérisé : photo ou scan d'une facture, d'un reçu, d'un bon de livraison.
///
/// Le contenu est stocké dans PostgreSQL (colonne bytea) et non sur le disque du
/// serveur : l'hébergement recrée le conteneur à chaque mise à jour, ce qui effacerait
/// les fichiers déposés. En base, ils suivent les sauvegardes comme le reste.
///
/// Une pièce est rattachée à un seul élément : soit une prévision journalière
/// (justificatifs du compte rendu), soit un décaissement, soit un rapport de travail.
/// </summary>
public class PieceJointe : BaseEntity
{
    /// <summary>Prévision dont cette pièce justifie le compte rendu.</summary>
    public long? PrevisionJournaliereId { get; set; }
    public PrevisionJournaliere? PrevisionJournaliere { get; set; }

    /// <summary>Décaissement que cette pièce justifie.</summary>
    public long? DecaissementId { get; set; }
    public Decaissement? Decaissement { get; set; }

    /// <summary>Rapport de travail auquel cette pièce est jointe.</summary>
    public long? RapportTravailId { get; set; }
    public RapportTravail? RapportTravail { get; set; }

    /// <summary>Nom d'origine du fichier, nettoyé.</summary>
    public string NomFichier { get; set; } = null!;

    /// <summary>Type MIME, ex : image/jpeg, application/pdf.</summary>
    public string TypeMime { get; set; } = null!;

    /// <summary>Taille en octets, conservée pour l'affichage sans charger le contenu.</summary>
    public long Taille { get; set; }

    /// <summary>Contenu binaire du fichier.</summary>
    public byte[] Contenu { get; set; } = Array.Empty<byte>();

    /// <summary>Ce que représente la pièce, ex : « Facture ciment FAC-2026-001 ».</summary>
    public string? Description { get; set; }

    /// <summary>Montant figurant sur la facture, pour rapprochement avec le décaissement.</summary>
    public decimal? MontantFacture { get; set; }

    /// <summary>Numéro de la facture ou du reçu.</summary>
    public string? NumeroPiece { get; set; }

    /// <summary>Fournisseur émetteur, en texte libre.</summary>
    public string? Emetteur { get; set; }

    public DateTime DateAjout { get; set; } = DateTime.UtcNow;
    public string? AjouteParId { get; set; }

    // --- Propriétés calculées ---

    /// <summary>Une image s'affiche en vignette, un PDF se télécharge.</summary>
    public bool EstImage => TypeMime.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    /// <summary>Taille lisible, ex : « 842 Ko ».</summary>
    public string TailleLisible => Taille switch
    {
        < 1024 => $"{Taille} o",
        < 1024 * 1024 => $"{Taille / 1024} Ko",
        _ => $"{Taille / (1024m * 1024m):N1} Mo"
    };
}
