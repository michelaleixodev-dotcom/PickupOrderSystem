using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickupOrderSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "status_history",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_anterior = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    status_novo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    alterado_em = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    alterado_por_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alterado_por_nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_status_history", x => x.id);
                    table.ForeignKey(
                        name: "FK_status_history_pickup_requests_solicitacao_id",
                        column: x => x.solicitacao_id,
                        principalTable: "pickup_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_status_history_alterado_em",
                table: "status_history",
                column: "alterado_em");

            migrationBuilder.CreateIndex(
                name: "IX_status_history_solicitacao_id",
                table: "status_history",
                column: "solicitacao_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "status_history");
        }
    }
}
