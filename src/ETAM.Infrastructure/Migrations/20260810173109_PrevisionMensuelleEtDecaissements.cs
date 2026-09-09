using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ETAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PrevisionMensuelleEtDecaissements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccuseNomSignataire",
                table: "Previsions",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccuseReceptionParId",
                table: "Previsions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAccuseReception",
                table: "Previsions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantAccuse",
                table: "Previsions",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MontantDecaisse",
                table: "Previsions",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "PrevisionMensuelleId",
                table: "Previsions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PrevisionPrecedenteId",
                table: "Previsions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReportVeille",
                table: "Previsions",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Decaissements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrevisionJournaliereId = table.Column<long>(type: "bigint", nullable: false),
                    PrevisionLigneId = table.Column<long>(type: "bigint", nullable: true),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Beneficiaire = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Motif = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    CompteBancaireId = table.Column<long>(type: "bigint", nullable: false),
                    BudgetConcerne = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    AccuseNom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DateAccuse = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decaissements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Decaissements_ComptesBancaires_CompteBancaireId",
                        column: x => x.CompteBancaireId,
                        principalTable: "ComptesBancaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Decaissements_PrevisionLignes_PrevisionLigneId",
                        column: x => x.PrevisionLigneId,
                        principalTable: "PrevisionLignes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Decaissements_Previsions_PrevisionJournaliereId",
                        column: x => x.PrevisionJournaliereId,
                        principalTable: "Previsions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrevisionsMensuelles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChantierId = table.Column<long>(type: "bigint", nullable: false),
                    PrevisionGlobaleId = table.Column<long>(type: "bigint", nullable: true),
                    Annee = table.Column<int>(type: "integer", nullable: false),
                    Mois = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    MontantPrevu = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ReportMoisPrecedent = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantConsomme = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrevisionMensuellePrecedenteId = table.Column<long>(type: "bigint", nullable: true),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    SoumisePar = table.Column<string>(type: "text", nullable: true),
                    DateSoumission = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValideeParId = table.Column<string>(type: "text", nullable: true),
                    DateValidation = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotifRefus = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DateCloture = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClotureeParId = table.Column<string>(type: "text", nullable: true),
                    Observation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrevisionsMensuelles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrevisionsMensuelles_Chantiers_ChantierId",
                        column: x => x.ChantierId,
                        principalTable: "Chantiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrevisionsMensuelles_PrevisionsGlobales_PrevisionGlobaleId",
                        column: x => x.PrevisionGlobaleId,
                        principalTable: "PrevisionsGlobales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrevisionsMensuelles_PrevisionsMensuelles_PrevisionMensuell~",
                        column: x => x.PrevisionMensuellePrecedenteId,
                        principalTable: "PrevisionsMensuelles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PiecesJointes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrevisionJournaliereId = table.Column<long>(type: "bigint", nullable: true),
                    DecaissementId = table.Column<long>(type: "bigint", nullable: true),
                    RapportTravailId = table.Column<long>(type: "bigint", nullable: true),
                    NomFichier = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TypeMime = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Taille = table.Column<long>(type: "bigint", nullable: false),
                    Contenu = table.Column<byte[]>(type: "bytea", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    MontantFacture = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    NumeroPiece = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    Emetteur = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    DateAjout = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AjouteParId = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PiecesJointes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PiecesJointes_Decaissements_DecaissementId",
                        column: x => x.DecaissementId,
                        principalTable: "Decaissements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PiecesJointes_Previsions_PrevisionJournaliereId",
                        column: x => x.PrevisionJournaliereId,
                        principalTable: "Previsions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PiecesJointes_RapportsTravail_RapportTravailId",
                        column: x => x.RapportTravailId,
                        principalTable: "RapportsTravail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrevisionMensuelleLignes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrevisionMensuelleId = table.Column<long>(type: "bigint", nullable: false),
                    Rubrique = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Designation = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrevisionGlobaleLigneId = table.Column<long>(type: "bigint", nullable: true),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrevisionMensuelleLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrevisionMensuelleLignes_PrevisionsGlobalesLignes_Prevision~",
                        column: x => x.PrevisionGlobaleLigneId,
                        principalTable: "PrevisionsGlobalesLignes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PrevisionMensuelleLignes_PrevisionsMensuelles_PrevisionMens~",
                        column: x => x.PrevisionMensuelleId,
                        principalTable: "PrevisionsMensuelles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Previsions_PrevisionMensuelleId",
                table: "Previsions",
                column: "PrevisionMensuelleId");

            migrationBuilder.CreateIndex(
                name: "IX_Previsions_PrevisionPrecedenteId",
                table: "Previsions",
                column: "PrevisionPrecedenteId");

            migrationBuilder.CreateIndex(
                name: "IX_Decaissements_CompteBancaireId",
                table: "Decaissements",
                column: "CompteBancaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Decaissements_Date",
                table: "Decaissements",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Decaissements_PrevisionJournaliereId_Date",
                table: "Decaissements",
                columns: new[] { "PrevisionJournaliereId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Decaissements_PrevisionLigneId",
                table: "Decaissements",
                column: "PrevisionLigneId");

            migrationBuilder.CreateIndex(
                name: "IX_PiecesJointes_DecaissementId",
                table: "PiecesJointes",
                column: "DecaissementId");

            migrationBuilder.CreateIndex(
                name: "IX_PiecesJointes_PrevisionJournaliereId",
                table: "PiecesJointes",
                column: "PrevisionJournaliereId");

            migrationBuilder.CreateIndex(
                name: "IX_PiecesJointes_RapportTravailId",
                table: "PiecesJointes",
                column: "RapportTravailId");

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionMensuelleLignes_PrevisionGlobaleLigneId",
                table: "PrevisionMensuelleLignes",
                column: "PrevisionGlobaleLigneId");

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionMensuelleLignes_PrevisionMensuelleId_Rubrique",
                table: "PrevisionMensuelleLignes",
                columns: new[] { "PrevisionMensuelleId", "Rubrique" });

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionsMensuelles_ChantierId_Annee_Mois",
                table: "PrevisionsMensuelles",
                columns: new[] { "ChantierId", "Annee", "Mois" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionsMensuelles_PrevisionGlobaleId",
                table: "PrevisionsMensuelles",
                column: "PrevisionGlobaleId");

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionsMensuelles_PrevisionMensuellePrecedenteId",
                table: "PrevisionsMensuelles",
                column: "PrevisionMensuellePrecedenteId");

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionsMensuelles_Reference",
                table: "PrevisionsMensuelles",
                column: "Reference",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Previsions_PrevisionsMensuelles_PrevisionMensuelleId",
                table: "Previsions",
                column: "PrevisionMensuelleId",
                principalTable: "PrevisionsMensuelles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Previsions_Previsions_PrevisionPrecedenteId",
                table: "Previsions",
                column: "PrevisionPrecedenteId",
                principalTable: "Previsions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Previsions_PrevisionsMensuelles_PrevisionMensuelleId",
                table: "Previsions");

            migrationBuilder.DropForeignKey(
                name: "FK_Previsions_Previsions_PrevisionPrecedenteId",
                table: "Previsions");

            migrationBuilder.DropTable(
                name: "PiecesJointes");

            migrationBuilder.DropTable(
                name: "PrevisionMensuelleLignes");

            migrationBuilder.DropTable(
                name: "Decaissements");

            migrationBuilder.DropTable(
                name: "PrevisionsMensuelles");

            migrationBuilder.DropIndex(
                name: "IX_Previsions_PrevisionMensuelleId",
                table: "Previsions");

            migrationBuilder.DropIndex(
                name: "IX_Previsions_PrevisionPrecedenteId",
                table: "Previsions");

            migrationBuilder.DropColumn(
                name: "AccuseNomSignataire",
                table: "Previsions");

            migrationBuilder.DropColumn(
                name: "AccuseReceptionParId",
                table: "Previsions");

            migrationBuilder.DropColumn(
                name: "DateAccuseReception",
                table: "Previsions");

            migrationBuilder.DropColumn(
                name: "MontantAccuse",
                table: "Previsions");

            migrationBuilder.DropColumn(
                name: "MontantDecaisse",
                table: "Previsions");

            migrationBuilder.DropColumn(
                name: "PrevisionMensuelleId",
                table: "Previsions");

            migrationBuilder.DropColumn(
                name: "PrevisionPrecedenteId",
                table: "Previsions");

            migrationBuilder.DropColumn(
                name: "ReportVeille",
                table: "Previsions");
        }
    }
}
