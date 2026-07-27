using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ETAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Tresorerie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Approvisionnements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChantierId = table.Column<long>(type: "bigint", nullable: false),
                    DateAppro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    Observation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrevisionJournaliereId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Approvisionnements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Approvisionnements_Chantiers_ChantierId",
                        column: x => x.ChantierId,
                        principalTable: "Chantiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Approvisionnements_Previsions_PrevisionJournaliereId",
                        column: x => x.PrevisionJournaliereId,
                        principalTable: "Previsions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ComptesBancaires",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Banque = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Numero = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Devise = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Solde = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    EstActif = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComptesBancaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Fournisseurs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Contact = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    Telephone = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    Adresse = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Nif = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fournisseurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovisionnementLignes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApprovisionnementId = table.Column<long>(type: "bigint", nullable: false),
                    Designation = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Categorie = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    TypeBudget = table.Column<int>(type: "integer", nullable: false),
                    MateriauId = table.Column<long>(type: "bigint", nullable: true),
                    Quantite = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PrixUnitaireEstime = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_ApprovisionnementLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovisionnementLignes_Approvisionnements_Approvisionnemen~",
                        column: x => x.ApprovisionnementId,
                        principalTable: "Approvisionnements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ApprovisionnementLignes_Materiaux_MateriauId",
                        column: x => x.MateriauId,
                        principalTable: "Materiaux",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DettesFournisseurs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FournisseurId = table.Column<long>(type: "bigint", nullable: false),
                    ChantierId = table.Column<long>(type: "bigint", nullable: true),
                    Libelle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MontantInitial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MontantPaye = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DettesFournisseurs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DettesFournisseurs_Chantiers_ChantierId",
                        column: x => x.ChantierId,
                        principalTable: "Chantiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DettesFournisseurs_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MouvementsBancaires",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompteBancaireId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Beneficiaire = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Motif = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Reference = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    ChantierId = table.Column<long>(type: "bigint", nullable: true),
                    FournisseurId = table.Column<long>(type: "bigint", nullable: true),
                    DetteFournisseurId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MouvementsBancaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MouvementsBancaires_Chantiers_ChantierId",
                        column: x => x.ChantierId,
                        principalTable: "Chantiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MouvementsBancaires_ComptesBancaires_CompteBancaireId",
                        column: x => x.CompteBancaireId,
                        principalTable: "ComptesBancaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MouvementsBancaires_DettesFournisseurs_DetteFournisseurId",
                        column: x => x.DetteFournisseurId,
                        principalTable: "DettesFournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MouvementsBancaires_Fournisseurs_FournisseurId",
                        column: x => x.FournisseurId,
                        principalTable: "Fournisseurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovisionnementLignes_ApprovisionnementId",
                table: "ApprovisionnementLignes",
                column: "ApprovisionnementId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovisionnementLignes_MateriauId",
                table: "ApprovisionnementLignes",
                column: "MateriauId");

            migrationBuilder.CreateIndex(
                name: "IX_Approvisionnements_ChantierId_DateAppro",
                table: "Approvisionnements",
                columns: new[] { "ChantierId", "DateAppro" });

            migrationBuilder.CreateIndex(
                name: "IX_Approvisionnements_PrevisionJournaliereId",
                table: "Approvisionnements",
                column: "PrevisionJournaliereId");

            migrationBuilder.CreateIndex(
                name: "IX_Approvisionnements_Reference",
                table: "Approvisionnements",
                column: "Reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DettesFournisseurs_ChantierId",
                table: "DettesFournisseurs",
                column: "ChantierId");

            migrationBuilder.CreateIndex(
                name: "IX_DettesFournisseurs_FournisseurId",
                table: "DettesFournisseurs",
                column: "FournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_Fournisseurs_Nom",
                table: "Fournisseurs",
                column: "Nom");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsBancaires_ChantierId",
                table: "MouvementsBancaires",
                column: "ChantierId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsBancaires_CompteBancaireId_Date",
                table: "MouvementsBancaires",
                columns: new[] { "CompteBancaireId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsBancaires_DetteFournisseurId",
                table: "MouvementsBancaires",
                column: "DetteFournisseurId");

            migrationBuilder.CreateIndex(
                name: "IX_MouvementsBancaires_FournisseurId",
                table: "MouvementsBancaires",
                column: "FournisseurId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovisionnementLignes");

            migrationBuilder.DropTable(
                name: "MouvementsBancaires");

            migrationBuilder.DropTable(
                name: "Approvisionnements");

            migrationBuilder.DropTable(
                name: "ComptesBancaires");

            migrationBuilder.DropTable(
                name: "DettesFournisseurs");

            migrationBuilder.DropTable(
                name: "Fournisseurs");
        }
    }
}
