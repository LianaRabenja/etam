using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ETAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMouvementMateriau : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MouvementsMateriau",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MateriauxId = table.Column<long>(type: "bigint", nullable: false),
                    DateMouvement = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BesoinOuObjectif = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    QuantiteEntree = table.Column<decimal>(type: "numeric(18,3)", nullable: false, defaultValue: 0m),
                    QuantiteSortie = table.Column<decimal>(type: "numeric(18,3)", nullable: false, defaultValue: 0m),
                    Motif = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    SoldeEnStock = table.Column<decimal>(type: "numeric(18,3)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MouvementsMateriau", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MouvementsMateriau_Materiaux_MateriauxId",
                        column: x => x.MateriauxId,
                        principalTable: "Materiaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // NOTE : les tables RapportsTravail (et ses lignes) ainsi que l'index
            // IX_DettesFournisseurs_Statut existent déjà dans la base (créés hors EF).
            // On ne crée donc ici QUE la table MouvementsMateriau, réellement manquante.

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsMateriau_MateriauxId_DateMouvement",
                table: "MouvementsMateriau",
                columns: new[] { "MateriauxId", "DateMouvement" },
                descending: new[] { false, true });

            // Index annexe : créé uniquement s'il n'existe pas déjà.
            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS \"IX_DettesFournisseurs_Statut\" ON \"DettesFournisseurs\" (\"Statut\");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MouvementsMateriau");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS \"IX_DettesFournisseurs_Statut\";");
        }
    }
}
