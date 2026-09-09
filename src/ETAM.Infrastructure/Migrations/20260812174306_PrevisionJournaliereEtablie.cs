using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ETAM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PrevisionJournaliereEtablie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PlanJournalierId",
                table: "Previsions",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlansJournaliers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PrevisionMensuelleId = table.Column<long>(type: "bigint", nullable: false),
                    ChantierId = table.Column<long>(type: "bigint", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MontantPrevu = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
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
                    table.PrimaryKey("PK_PlansJournaliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlansJournaliers_Chantiers_ChantierId",
                        column: x => x.ChantierId,
                        principalTable: "Chantiers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PlansJournaliers_PrevisionsMensuelles_PrevisionMensuelleId",
                        column: x => x.PrevisionMensuelleId,
                        principalTable: "PrevisionsMensuelles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Previsions_PlanJournalierId",
                table: "Previsions",
                column: "PlanJournalierId");

            migrationBuilder.CreateIndex(
                name: "IX_PlansJournaliers_ChantierId_Date",
                table: "PlansJournaliers",
                columns: new[] { "ChantierId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlansJournaliers_PrevisionMensuelleId",
                table: "PlansJournaliers",
                column: "PrevisionMensuelleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Previsions_PlansJournaliers_PlanJournalierId",
                table: "Previsions",
                column: "PlanJournalierId",
                principalTable: "PlansJournaliers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Previsions_PlansJournaliers_PlanJournalierId",
                table: "Previsions");

            migrationBuilder.DropTable(
                name: "PlansJournaliers");

            migrationBuilder.DropIndex(
                name: "IX_Previsions_PlanJournalierId",
                table: "Previsions");

            migrationBuilder.DropColumn(
                name: "PlanJournalierId",
                table: "Previsions");
        }
    }
}
