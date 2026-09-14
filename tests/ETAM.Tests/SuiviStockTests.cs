using ETAM.Domain.Entities;
using Xunit;

namespace ETAM.Tests;

/// <summary>
/// Suivi du stock : objectif → reçu → utilisé.
/// La notion de « commandé » a été retirée du suivi ; l'avancement se mesure
/// désormais sur l'objectif (le besoin total du chantier).
/// </summary>
public class SuiviStockTests
{
    [Fact]
    public void Le_stock_disponible_est_le_recu_moins_l_utilise()
    {
        var m = new Materiau { Besoin = 669m, QuantiteRecue = 400m, QuantiteUtilisee = 150m };

        Assert.Equal(250m, m.StockDisponible);
    }

    [Fact]
    public void L_avancement_se_mesure_sur_l_objectif_pas_sur_le_commande()
    {
        // 669 sacs nécessaires, 400 reçus → environ 60 % de l'objectif.
        var m = new Materiau { Besoin = 669m, QuantiteRecue = 400m };

        Assert.InRange(m.PourcentageReception, 59.5, 60.5);
        Assert.Equal(269m, m.ResteARecevoir);
    }

    [Fact]
    public void Le_reste_a_recevoir_ne_devient_pas_negatif_si_on_recoit_plus_que_prevu()
    {
        var m = new Materiau { Besoin = 100m, QuantiteRecue = 130m };

        Assert.Equal(0m, m.ResteARecevoir);
    }

    [Fact]
    public void Un_article_sans_objectif_n_affiche_pas_un_avancement_absurde()
    {
        // Division par zéro : l'avancement doit valoir 0, pas planter.
        var m = new Materiau { Besoin = 0m, QuantiteRecue = 50m };

        Assert.Equal(0d, m.PourcentageReception);
    }

    [Fact]
    public void Le_stock_est_critique_quand_il_est_epuise()
    {
        var epuise = new Materiau { QuantiteRecue = 100m, QuantiteUtilisee = 100m, SeuilMinimal = 10m };
        var faible = new Materiau { QuantiteRecue = 100m, QuantiteUtilisee = 95m, SeuilMinimal = 10m };
        var bon    = new Materiau { QuantiteRecue = 100m, QuantiteUtilisee = 20m, SeuilMinimal = 10m };

        Assert.True(epuise.EstStockCritique);
        Assert.True(faible.EstStockFaible);
        Assert.False(bon.EstStockFaible);
        Assert.False(bon.EstStockCritique);
    }
}
