using ETAM.Domain.Entities;
using Xunit;

namespace ETAM.Tests;

/// <summary>
/// Les quatre règles qui font la valeur du logiciel.
///
/// Elles ne sont écrites nulle part ailleurs que dans le code : si quelqu'un les casse
/// en modifiant autre chose, rien ne l'avertit — sauf ces tests. Ils tournent en quelques
/// millisecondes et ne dépendent ni d'une base de données, ni d'un serveur.
///
/// Pour les lancer :   dotnet test
/// </summary>
public class ReglesFinancieresTests
{
    // ------------------------------------------------------------------
    //  RÈGLE 1 — Le fléchage réserve l'argent, il ne le sort pas de la banque.
    // ------------------------------------------------------------------

    [Fact]
    public void Flecher_ne_retire_pas_l_argent_de_la_banque()
    {
        // 30 millions en banque, 10 millions fléchés vers le budget matériel.
        var chantier = new Chantier { MaterielTransfere = 10_000_000m, Consommation = 0m };
        const decimal soldeBancaire = 30_000_000m;

        // Le budget matériel a bien reçu les 10 millions...
        Assert.Equal(10_000_000m, chantier.MaterielDisponible);

        // ...mais il reste 20 millions fléchables : le solde bancaire n'a pas bougé,
        // seuls les 10 millions déjà réservés sont retirés du disponible.
        Assert.Equal(20_000_000m, chantier.DisponibleAFlecher(soldeBancaire));
    }

    // ------------------------------------------------------------------
    //  RÈGLE 2 — On ne peut pas flécher deux fois le même argent.
    // ------------------------------------------------------------------

    [Fact]
    public void On_ne_peut_pas_flecher_plus_que_le_solde_disponible()
    {
        // Tout le solde est déjà fléché, rien n'est encore consommé.
        var chantier = new Chantier { MaterielTransfere = 30_000_000m, Consommation = 0m };

        // Plus rien à flécher, même si la banque affiche toujours 30 millions.
        Assert.Equal(0m, chantier.DisponibleAFlecher(30_000_000m));
    }

    [Fact]
    public void Le_disponible_a_flecher_n_est_jamais_negatif()
    {
        // Cas limite : plus fléché que ce qui reste en banque (sorties déjà passées).
        var chantier = new Chantier { MaterielTransfere = 50_000_000m, Consommation = 0m };

        Assert.Equal(0m, chantier.DisponibleAFlecher(10_000_000m));
    }

    // ------------------------------------------------------------------
    //  RÈGLE 3 — Ce qui est consommé libère du disponible.
    // ------------------------------------------------------------------

    [Fact]
    public void L_argent_consomme_n_est_plus_compte_comme_reserve()
    {
        // 10 millions fléchés, 4 déjà dépensés : seuls 6 restent réservés.
        var chantier = new Chantier { MaterielTransfere = 10_000_000m, Consommation = 4_000_000m };

        Assert.Equal(6_000_000m, chantier.MaterielReserve);

        // La banque ne porte plus que 26 millions (les 4 sont sortis) :
        // 26 − 6 réservés = 20 millions encore fléchables.
        Assert.Equal(20_000_000m, chantier.DisponibleAFlecher(26_000_000m));
    }

    [Fact]
    public void Une_consommation_superieure_au_fleche_ne_cree_pas_d_argent()
    {
        // Situation anormale (données héritées) : consommé > fléché.
        // La réserve doit tomber à zéro, jamais passer en négatif — sinon elle
        // AUGMENTERAIT le disponible à flécher, et créerait de l'argent.
        var chantier = new Chantier { MaterielTransfere = 5_000_000m, Consommation = 8_000_000m };

        Assert.Equal(0m, chantier.MaterielReserve);
        Assert.Equal(10_000_000m, chantier.DisponibleAFlecher(10_000_000m));
    }

    // ------------------------------------------------------------------
    //  RÈGLE 4 — Le budget d'un projet, c'est le marché moins le bénéfice.
    // ------------------------------------------------------------------

    [Fact]
    public void Le_budget_du_projet_exclut_le_benefice()
    {
        var chantier = new Chantier { MontantMarche = 150_000_000m, Benefice = 80_000_000m };

        Assert.Equal(70_000_000m, chantier.BudgetProjet);
    }

    // ------------------------------------------------------------------
    //  La même règle de fléchage s'applique au Budget Comptes de l'entreprise.
    // ------------------------------------------------------------------

    [Fact]
    public void Le_budget_comptes_suit_la_meme_regle_de_flechage()
    {
        var budget = new BudgetCompte
        {
            Annee = 2026,
            MontantInitial = 50_000_000m,
            MontantTransfere = 20_000_000m,
            MontantConsomme = 5_000_000m
        };

        Assert.Equal(15_000_000m, budget.MontantReserve);
        Assert.Equal(25_000_000m, budget.DisponibleAFlecher(40_000_000m));
    }
}
