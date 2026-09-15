using ETAM.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETAM.Infrastructure.Migrations
{
    /// <summary>
    /// Clôture de période : le reste non utilisé d'un chantier retourne en banque.
    ///
    /// Trois colonnes sur "Previsions" :
    ///   MontantRestitue  — ce qui est reparti en banque à la clôture
    ///   DateRestitution  — quand (nulle tant que la période est ouverte)
    ///   RestitueParId    — quel Administrateur a clôturé
    ///
    /// « IF NOT EXISTS » rend la migration rejouable sans risque : elle est sans
    /// effet là où les colonnes existent déjà. Les prévisions existantes partent
    /// avec MontantRestitue = 0 et DateRestitution nulle, donc leur période reste
    /// ouverte et leur reste continue de se reporter — aucun comportement changé
    /// tant que personne ne clôture.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260915200000_CloturePeriodeRetourEnBanque")]
    public partial class CloturePeriodeRetourEnBanque : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""Previsions""
    ADD COLUMN IF NOT EXISTS ""MontantRestitue"" numeric(18,2) NOT NULL DEFAULT 0;

ALTER TABLE ""Previsions""
    ADD COLUMN IF NOT EXISTS ""DateRestitution"" timestamp with time zone;

ALTER TABLE ""Previsions""
    ADD COLUMN IF NOT EXISTS ""RestitueParId"" text;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
ALTER TABLE ""Previsions"" DROP COLUMN IF EXISTS ""RestitueParId"";
ALTER TABLE ""Previsions"" DROP COLUMN IF EXISTS ""DateRestitution"";
ALTER TABLE ""Previsions"" DROP COLUMN IF EXISTS ""MontantRestitue"";
");
        }
    }
}
