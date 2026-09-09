using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ETAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PrevisionGlobale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PrevisionsGlobales",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChantierId = table.Column<long>(type: "bigint", nullable: false),
                    Reference = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Statut = table.Column<int>(type: "integer", nullable: false),
                    Observation = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SoumisePar = table.Column<string>(type: "text", nullable: true),
                    DateSoumission = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValideeParRfId = table.Column<string>(type: "text", nullable: true),
                    DateValidationRf = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValideeParAdminId = table.Column<string>(type: "text", nullable: true),
                    DateValidationAdmin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotifRefus = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DateMiseEnBanque = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrevisionsGlobales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrevisionsGlobales_Chantiers_ChantierId",
                        column: x => x.ChantierId,
                        principalTable: "Chantiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrevisionsGlobalesLignes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrevisionGlobaleId = table.Column<long>(type: "bigint", nullable: false),
                    Rubrique = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Designation = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Unite = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Quantite = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PrixUnitaire = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Observation = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrevisionsGlobalesLignes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrevisionsGlobalesLignes_PrevisionsGlobales_PrevisionGlobal~",
                        column: x => x.PrevisionGlobaleId,
                        principalTable: "PrevisionsGlobales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionsGlobales_ChantierId",
                table: "PrevisionsGlobales",
                column: "ChantierId");

            migrationBuilder.CreateIndex(
                name: "IX_PrevisionsGlobalesLignes_PrevisionGlobaleId",
                table: "PrevisionsGlobalesLignes",
                column: "PrevisionGlobaleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrevisionsGlobalesLignes");

            migrationBuilder.DropTable(
                name: "PrevisionsGlobales");
        }
    }
}
