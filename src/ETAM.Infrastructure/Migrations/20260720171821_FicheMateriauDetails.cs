using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FicheMateriauDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SoldeSurBesoin",
                table: "MouvementsMateriau",
                type: "numeric(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Besoin",
                table: "Materiaux",
                type: "numeric(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Localite",
                table: "Materiaux",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoldeSurBesoin",
                table: "MouvementsMateriau");

            migrationBuilder.DropColumn(
                name: "Besoin",
                table: "Materiaux");

            migrationBuilder.DropColumn(
                name: "Localite",
                table: "Materiaux");
        }
    }
}
