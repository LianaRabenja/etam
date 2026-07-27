using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MarcheEtPrevisionGlobale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PrevisionGlobaleLigneId",
                table: "PrevisionLignes",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Benefice",
                table: "Chantiers",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantMarche",
                table: "Chantiers",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionLignes_PrevisionGlobaleLigneId",
                table: "PrevisionLignes",
                column: "PrevisionGlobaleLigneId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrevisionLignes_PrevisionsGlobalesLignes_PrevisionGlobaleLi~",
                table: "PrevisionLignes",
                column: "PrevisionGlobaleLigneId",
                principalTable: "PrevisionsGlobalesLignes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrevisionLignes_PrevisionsGlobalesLignes_PrevisionGlobaleLi~",
                table: "PrevisionLignes");

            migrationBuilder.DropIndex(
                name: "IX_PrevisionLignes_PrevisionGlobaleLigneId",
                table: "PrevisionLignes");

            migrationBuilder.DropColumn(
                name: "PrevisionGlobaleLigneId",
                table: "PrevisionLignes");

            migrationBuilder.DropColumn(
                name: "Benefice",
                table: "Chantiers");

            migrationBuilder.DropColumn(
                name: "MontantMarche",
                table: "Chantiers");
        }
    }
}
