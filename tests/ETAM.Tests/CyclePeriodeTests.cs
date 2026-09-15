using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using Xunit;

namespace ETAM.Tests;

/// <summary>
/// Le cycle de la caisse de chantier : les journées se cumulent, puis la période
/// est clôturée et le reste retourne en banque.
///
///   Lundi 5 M demandés, 3 M dépensés        → reste 2 M
///   Mardi 4 M demandés + 2 M reportés       → plafond 6 M
///   ...
///   Clôture : ce qui reste repart en banque → la période suivante part de zéro
///
/// Ces règles ne sont écrites nulle part ailleurs que dans le code. Si quelqu'un les
/// casse en modifiant autre chose, seuls ces tests l'avertiront.
///
/// Pour les lancer :   dotnet test
/// </summary>
public class CyclePeriodeTests
{
    /// <summary>Fabrique une journée ouverte avec une seule ligne du montant voulu.</summary>
    private static PrevisionJournaliere Journee(decimal demande, decimal report = 0m)
    {
        var p = new PrevisionJournaliere
        {
            Statut = StatutPrevision.Executee,
            DateAccuseReception = DateTime.UtcNow,
            ReportVeille = report
        };
        p.Lignes.Add(new PrevisionLigne
        {
            Designation = "Journée",
            Quantite = 1m,
            PrixUnitaireEstime = demande
        });
        return p;
    }

    // ------------------------------------------------------------------
    //  Le cumul d'un jour sur l'autre
    // ------------------------------------------------------------------

    [Fact]
    public void Le_reste_de_la_veille_augmente_le_plafond_du_jour()
    {
        // Lundi : 5 M demandés, 3 M dépensés → il reste 2 M.
        var lundi = Journee(5_000_000m);
        lundi.MontantDecaisse = 3_000_000m;

        Assert.Equal(5_000_000m, lundi.PlafondDuJour);
        Assert.Equal(2_000_000m, lundi.Reliquat);

        // Mardi : 4 M demandés, plus les 2 M de lundi → 6 M dépensables.
        var mardi = Journee(4_000_000m, report: lundi.Reliquat);

        Assert.Equal(6_000_000m, mardi.PlafondDuJour);
    }

    [Fact]
    public void Le_cumul_de_la_periode_vaut_le_sorti_moins_le_depense()
    {
        // Trois journées : 5 + 4 + 3 M sortis de la banque, 3 + 5 + 1 M dépensés.
        var lundi = Journee(5_000_000m);
        lundi.MontantDecaisse = 3_000_000m;

        var mardi = Journee(4_000_000m, report: lundi.Reliquat);
        mardi.MontantDecaisse = 5_000_000m;

        var mercredi = Journee(3_000_000m, report: mardi.Reliquat);
        mercredi.MontantDecaisse = 1_000_000m;

        var sorti = 5_000_000m + 4_000_000m + 3_000_000m;
        var depense = 3_000_000m + 5_000_000m + 1_000_000m;

        // Le reste de la période ne s'obtient PAS en additionnant les restes
        // journaliers : celui de lundi est déjà compté dans le plafond de mardi.
        // Il vaut « sorti de la banque moins dépensé », et retombe exactement sur
        // le reste de la dernière journée.
        Assert.Equal(3_000_000m, sorti - depense);
        Assert.Equal(sorti - depense, mercredi.Reliquat);
    }

    // ------------------------------------------------------------------
    //  La clôture de période
    // ------------------------------------------------------------------

    [Fact]
    public void La_cloture_vide_le_reste_de_la_journee()
    {
        var vendredi = Journee(5_000_000m);
        vendredi.MontantDecaisse = 3_000_000m;
        Assert.Equal(2_000_000m, vendredi.Reliquat);

        // L'administrateur clôture : les 2 M repartent en banque.
        vendredi.MontantRestitue = 2_000_000m;
        vendredi.DateRestitution = DateTime.UtcNow;

        Assert.True(vendredi.EstRestituee);
        Assert.Equal(0m, vendredi.Reliquat);
    }

    [Fact]
    public void Apres_cloture_plus_aucune_sortie_n_est_possible()
    {
        var vendredi = Journee(5_000_000m);
        vendredi.MontantDecaisse = 3_000_000m;

        // Tant que la période est ouverte, on peut encore dépenser les 2 M.
        Assert.True(vendredi.PeutDecaisser);

        vendredi.MontantRestitue = 2_000_000m;
        vendredi.DateRestitution = DateTime.UtcNow;

        // L'argent est en banque : la caisse du chantier est fermée.
        Assert.False(vendredi.PeutDecaisser);
    }

    [Fact]
    public void La_periode_suivante_repart_de_zero()
    {
        var vendredi = Journee(5_000_000m);
        vendredi.MontantDecaisse = 3_000_000m;
        vendredi.MontantRestitue = 2_000_000m;
        vendredi.DateRestitution = DateTime.UtcNow;

        // C'est ce calcul que fait PrevisionService.ExecuterAsync pour établir
        // le report : plafond de la veille, moins ce qui a été dépensé, moins ce
        // qui est reparti en banque.
        var report = vendredi.PlafondDuJour - vendredi.MontantDecaisse - vendredi.MontantRestitue;

        Assert.Equal(0m, report);

        // Le lundi suivant ne demande donc que son propre montant.
        var lundiSuivant = Journee(4_000_000m, report: report);
        Assert.Equal(4_000_000m, lundiSuivant.PlafondDuJour);
    }

    [Fact]
    public void Une_cloture_sans_reste_ferme_quand_meme_la_periode()
    {
        // Tout a été dépensé : rien ne revient en banque, mais la période se ferme.
        var vendredi = Journee(5_000_000m);
        vendredi.MontantDecaisse = 5_000_000m;

        vendredi.MontantRestitue = 0m;
        vendredi.DateRestitution = DateTime.UtcNow;

        Assert.True(vendredi.EstRestituee);
        Assert.Equal(0m, vendredi.Reliquat);
        Assert.False(vendredi.PeutDecaisser);
    }

    [Fact]
    public void Le_pourcentage_consomme_suit_le_plafond_reel()
    {
        // Les alertes 50 / 80 / 90 % se calculent sur le plafond du jour,
        // report compris — pas sur le seul montant demandé.
        var mardi = Journee(4_000_000m, report: 2_000_000m);
        mardi.MontantDecaisse = 3_000_000m;

        Assert.Equal(6_000_000m, mardi.PlafondDuJour);
        Assert.Equal(50d, mardi.PourcentageDecaisse, 3);
    }
}
