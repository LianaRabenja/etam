using ETAM.Domain.Entities;
using ETAM.Domain.Enums;
using Xunit;

namespace ETAM.Tests;

/// <summary>
/// Le cycle de l'argent d'un chantier, dans sa règle actuelle :
/// EXÉCUTER UNE PRÉVISION, C'EST DÉPENSER.
///
/// L'argent quitte la banque à l'exécution et il est dépensé dans le même geste.
/// Il ne reste donc rien entre les mains de personne : plus de reliquat journalier,
/// plus de report d'un jour sur l'autre, plus de retour de caisse à la clôture.
///
/// Le cumul se fait au niveau de la SEMAINE, contre la prévision de la semaine,
/// qui tient le rôle de l'argent réel :
///
///   Prévision semaine 20 M
///   Lundi exécuté   5 M  → cumul  5 M, reste 15 M
///   Mardi exécuté   4 M  → cumul  9 M, reste 11 M
///   ...
///   Clôture : on constate l'écart. Le reste n'est jamais sorti de la banque,
///             il n'y a donc rien à y remettre.
///
/// Ces règles ne sont écrites nulle part ailleurs que dans le code. Si quelqu'un les
/// casse en modifiant autre chose, seuls ces tests l'avertiront.
///
/// Pour les lancer :   dotnet test
/// </summary>
public class CyclePeriodeTests
{
    /// <summary>Fabrique une journée exécutée avec une seule ligne du montant voulu.</summary>
    private static PrevisionJournaliere Journee(decimal montant)
    {
        var p = new PrevisionJournaliere
        {
            Statut = StatutPrevision.Executee,
            DateAccuseReception = DateTime.UtcNow
        };
        p.Lignes.Add(new PrevisionLigne
        {
            Designation = "Journée",
            Quantite = 1m,
            PrixUnitaireEstime = montant
        });
        return p;
    }

    /// <summary>
    /// Reproduit le calcul de PrevisionService.EvaluerPlanSemaineAsync : le palier
    /// d'alerte atteint par la semaine. 0 = rien à signaler, 100 = dépassement.
    /// </summary>
    private static int Palier(decimal prevuSemaine, decimal depenseSemaine)
    {
        if (prevuSemaine <= 0) return 0;
        if (depenseSemaine > prevuSemaine) return 100;

        var pct = (double)(depenseSemaine / prevuSemaine) * 100d;
        return pct >= 90 ? 90 : pct >= 80 ? 80 : 0;
    }

    // ------------------------------------------------------------------
    //  Exécuter, c'est dépenser
    // ------------------------------------------------------------------

    [Fact]
    public void Une_journee_executee_ne_laisse_aucun_reliquat()
    {
        // 5 M demandés et exécutés : les 5 M sont sortis de la banque ET dépensés.
        var lundi = Journee(5_000_000m);

        Assert.Equal(5_000_000m, lundi.PlafondDuJour);

        // Rien ne reste dehors, même si aucun décaissement n'a été saisi. C'est le
        // cœur de la règle : l'ancien calcul renvoyait ici les 5 M, et la clôture
        // les recréditait au compte — de l'argent inventé.
        Assert.Equal(0m, lundi.Reliquat);
    }

    [Fact]
    public void Aucun_report_ne_se_traine_d_un_jour_sur_l_autre()
    {
        // Le lundi est exécuté, donc dépensé. Le mardi ne demande que son montant.
        var lundi = Journee(5_000_000m);
        var mardi = Journee(4_000_000m);

        Assert.Equal(0m, mardi.ReportVeille);
        Assert.Equal(4_000_000m, mardi.PlafondDuJour);

        // Le plafond du mardi ne dépend en rien du lundi : c'est ce qui empêche le
        // plafond de s'empiler à l'infini (jour3 = jour1 + jour2 + jour3).
        Assert.Equal(5_000_000m, lundi.PlafondDuJour);
    }

    [Fact]
    public void Une_journee_non_executee_n_a_rien_depense()
    {
        // Une prévision encore en brouillon n'a fait sortir aucun argent : son
        // montant reste entièrement à engager.
        var brouillon = Journee(5_000_000m);
        brouillon.Statut = StatutPrevision.Brouillon;

        Assert.Equal(5_000_000m, brouillon.Reliquat);
    }

    // ------------------------------------------------------------------
    //  Le cumul de la semaine contre sa prévision
    // ------------------------------------------------------------------

    [Fact]
    public void Le_cumul_de_la_semaine_est_la_somme_des_journees_executees()
    {
        var prevuSemaine = 20_000_000m;

        var journees = new[] { Journee(5_000_000m), Journee(4_000_000m), Journee(3_000_000m) };
        var depense = journees.Sum(j => j.Total);

        Assert.Equal(12_000_000m, depense);
        Assert.Equal(8_000_000m, prevuSemaine - depense);
    }

    [Fact]
    public void Le_reste_de_la_semaine_n_est_jamais_sorti_de_la_banque()
    {
        // 20 M prévus, 12 M dépensés : les 8 M restants sont toujours sur le compte.
        // Aucune journée ne les porte — la somme des reliquats est nulle.
        var prevuSemaine = 20_000_000m;
        var journees = new[] { Journee(5_000_000m), Journee(4_000_000m), Journee(3_000_000m) };

        Assert.Equal(0m, journees.Sum(j => j.Reliquat));
        Assert.Equal(8_000_000m, prevuSemaine - journees.Sum(j => j.Total));
    }

    [Fact]
    public void Un_reste_negatif_signale_un_depassement_de_la_prevision()
    {
        var prevuSemaine = 10_000_000m;
        var journees = new[] { Journee(6_000_000m), Journee(7_000_000m) };

        var reste = prevuSemaine - journees.Sum(j => j.Total);

        Assert.Equal(-3_000_000m, reste);
        Assert.True(reste < 0);
    }

    [Fact]
    public void Sans_prevision_de_semaine_il_n_y_a_rien_a_comparer()
    {
        // Aucun montant saisi dans la prévision semaine : on ne crie pas au
        // dépassement, on n'a simplement pas de référence.
        Assert.Equal(0, Palier(0m, 5_000_000m));
    }

    // ------------------------------------------------------------------
    //  Les alertes de seuil
    // ------------------------------------------------------------------

    // Les arguments d'attribut ne peuvent pas être des decimal — on passe des int
    // et on convertit dans le test.
    [Theory]
    [InlineData(10_000_000, 7_000_000, 0)]    // 70 % : on ne dit rien
    [InlineData(10_000_000, 8_000_000, 80)]   // 80 % pile : on prévient
    [InlineData(10_000_000, 8_900_000, 80)]   // 89 % : toujours le palier 80
    [InlineData(10_000_000, 9_000_000, 90)]   // 90 % : palier haut
    [InlineData(10_000_000, 10_000_000, 90)]  // pile la prévision : pas encore dépassée
    [InlineData(10_000_000, 10_000_001, 100)] // un ariary de trop : dépassement
    public void Les_paliers_d_alerte_suivent_la_prevision_de_la_semaine(
        int prevu, int depense, int palierAttendu)
    {
        Assert.Equal(palierAttendu, Palier(prevu, depense));
    }

    // ------------------------------------------------------------------
    //  La clôture de la semaine
    // ------------------------------------------------------------------

    [Fact]
    public void La_cloture_ne_rend_rien_en_banque()
    {
        var vendredi = Journee(5_000_000m);

        // L'administrateur clôture la semaine : on arrête les comptes, point.
        vendredi.MontantRestitue = 0m;
        vendredi.DateRestitution = DateTime.UtcNow;

        Assert.True(vendredi.EstRestituee);

        // RIEN n'est restitué : l'argent non dépensé de la semaine n'a jamais
        // quitté le compte. Un montant non nul ici recréditerait le compte deux fois.
        Assert.Equal(0m, vendredi.MontantRestitue);
    }

    [Fact]
    public void Une_semaine_cloturee_ne_se_ventile_plus()
    {
        var vendredi = Journee(5_000_000m);

        // Tant que la semaine est ouverte, on peut encore détailler à qui les
        // 5 M ont servi — ce détail ne sort aucun argent, il fait descendre le stock.
        Assert.True(vendredi.PeutDecaisser);

        vendredi.DateRestitution = DateTime.UtcNow;

        // Comptes arrêtés : plus aucune écriture sur cette journée.
        Assert.False(vendredi.PeutDecaisser);
    }

    [Fact]
    public void On_ne_ventile_pas_plus_que_le_montant_de_la_journee()
    {
        var lundi = Journee(5_000_000m);

        // 3 M ventilés : il reste 2 M à attribuer.
        lundi.MontantDecaisse = 3_000_000m;
        Assert.True(lundi.PeutDecaisser);
        Assert.Equal(2_000_000m, lundi.PlafondDuJour - lundi.MontantDecaisse);

        // Tout est attribué : plus rien à ventiler, mais la dépense, elle, était
        // déjà faite depuis l'exécution.
        lundi.MontantDecaisse = 5_000_000m;
        Assert.False(lundi.PeutDecaisser);
        Assert.Equal(0m, lundi.Reliquat);
    }

    [Fact]
    public void La_semaine_suivante_repart_de_zero()
    {
        var vendredi = Journee(5_000_000m);
        vendredi.DateRestitution = DateTime.UtcNow;

        // Le lundi suivant ne demande que son propre montant : aucun héritage.
        var lundiSuivant = Journee(4_000_000m);

        Assert.Equal(0m, lundiSuivant.ReportVeille);
        Assert.Equal(4_000_000m, lundiSuivant.PlafondDuJour);
    }

    [Fact]
    public void Le_pourcentage_ventile_suit_le_montant_du_jour()
    {
        // Le compteur de ventilation se calcule sur le montant de la journée.
        var mardi = Journee(6_000_000m);
        mardi.MontantDecaisse = 3_000_000m;

        Assert.Equal(6_000_000m, mardi.PlafondDuJour);
        Assert.Equal(50d, mardi.PourcentageDecaisse, 3);
    }
}
