using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RolesEtTransferts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DetteFournisseurId",
                table: "PrevisionLignes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DemandePar",
                table: "MouvementsBancaires",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EstValide",
                table: "MouvementsBancaires",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<long>(
                name: "ChantierId",
                table: "ComptesBancaires",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "ComptesBancaires",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterielTransfere",
                table: "Chantiers",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantTransfere",
                table: "BudgetsComptes",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "DetteFournisseurId",
                table: "ApprovisionnementLignes",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionLignes_DetteFournisseurId",
                table: "PrevisionLignes",
                column: "DetteFournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_ComptesBancaires_ChantierId",
                table: "ComptesBancaires",
                column: "ChantierId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovisionnementLignes_DetteFournisseurId",
                table: "ApprovisionnementLignes",
                column: "DetteFournisseurId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApprovisionnementLignes_DettesFournisseurs_DetteFournisseur~",
                table: "ApprovisionnementLignes",
                column: "DetteFournisseurId",
                principalTable: "DettesFournisseurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ComptesBancaires_Chantiers_ChantierId",
                table: "ComptesBancaires",
                column: "ChantierId",
                principalTable: "Chantiers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PrevisionLignes_DettesFournisseurs_DetteFournisseurId",
                table: "PrevisionLignes",
                column: "DetteFournisseurId",
                principalTable: "DettesFournisseurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApprovisionnementLignes_DettesFournisseurs_DetteFournisseur~",
                table: "ApprovisionnementLignes");

            migrationBuilder.DropForeignKey(
                name: "FK_ComptesBancaires_Chantiers_ChantierId",
                table: "ComptesBancaires");

            migrationBuilder.DropForeignKey(
                name: "FK_PrevisionLignes_DettesFournisseurs_DetteFournisseurId",
                table: "PrevisionLignes");

            migrationBuilder.DropIndex(
                name: "IX_PrevisionLignes_DetteFournisseurId",
                table: "PrevisionLignes");

            migrationBuilder.DropIndex(
                name: "IX_ComptesBancaires_ChantierId",
                table: "ComptesBancaires");

            migrationBuilder.DropIndex(
                name: "IX_ApprovisionnementLignes_DetteFournisseurId",
                table: "ApprovisionnementLignes");

            migrationBuilder.DropColumn(
                name: "DetteFournisseurId",
                table: "PrevisionLignes");

            migrationBuilder.DropColumn(
                name: "DemandePar",
                table: "MouvementsBancaires");

            migrationBuilder.DropColumn(
                name: "EstValide",
                table: "MouvementsBancaires");

            migrationBuilder.DropColumn(
                name: "ChantierId",
                table: "ComptesBancaires");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "ComptesBancaires");

            migrationBuilder.DropColumn(
                name: "MaterielTransfere",
                table: "Chantiers");

            migrationBuilder.DropColumn(
                name: "MontantTransfere",
                table: "BudgetsComptes");

            migrationBuilder.DropColumn(
                name: "DetteFournisseurId",
                table: "ApprovisionnementLignes");
        }
    }
}
