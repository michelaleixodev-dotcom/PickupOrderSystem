using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PickupOrderSystem.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "drivers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    cnh = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    data_admissao = table.Column<DateOnly>(type: "date", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_drivers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    senha_hash = table.Column<string>(type: "text", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    cnpj = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    endereco = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    placa = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    capacidade_kg = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    capacidade_m3 = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ano_fabricacao = table.Column<int>(type: "integer", nullable: false),
                    ativo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ultima_manutencao = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "pickup_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    numero_identificacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    remetente = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    endereco_coleta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    destinatario = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    endereco_entrega = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    data_solicitacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    data_coleta_prevista = table.Column<DateOnly>(type: "date", nullable: false),
                    prioridade = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false, defaultValue: "Aberta"),
                    observacoes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pickup_requests", x => x.id);
                    table.ForeignKey(
                        name: "FK_pickup_requests_users_usuario_id",
                        column: x => x.usuario_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    motorista_id = table.Column<Guid>(type: "uuid", nullable: false),
                    veiculo_id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_atribuicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    data_inicio_real = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    data_conclusao_real = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    observacoes_motorista = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_assignments_drivers_motorista_id",
                        column: x => x.motorista_id,
                        principalTable: "drivers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assignments_pickup_requests_solicitacao_id",
                        column: x => x.solicitacao_id,
                        principalTable: "pickup_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_assignments_vehicles_veiculo_id",
                        column: x => x.veiculo_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "occurrences",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    solicitacao_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo = table.Column<string>(type: "text", nullable: false),
                    descricao = table.Column<string>(type: "text", nullable: false),
                    data_hora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    usuario_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    resolvida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    observacoes_resolucao = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_occurrences", x => x.id);
                    table.ForeignKey(
                        name: "FK_occurrences_pickup_requests_solicitacao_id",
                        column: x => x.solicitacao_id,
                        principalTable: "pickup_requests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_assignments_data_atribuicao",
                table: "assignments",
                column: "data_atribuicao");

            migrationBuilder.CreateIndex(
                name: "IX_assignments_motorista_id",
                table: "assignments",
                column: "motorista_id");

            migrationBuilder.CreateIndex(
                name: "IX_assignments_solicitacao_id",
                table: "assignments",
                column: "solicitacao_id");

            migrationBuilder.CreateIndex(
                name: "IX_assignments_veiculo_id",
                table: "assignments",
                column: "veiculo_id");

            migrationBuilder.CreateIndex(
                name: "IX_drivers_cnh",
                table: "drivers",
                column: "cnh",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_occurrences_data_hora",
                table: "occurrences",
                column: "data_hora");

            migrationBuilder.CreateIndex(
                name: "IX_occurrences_resolvida",
                table: "occurrences",
                column: "resolvida");

            migrationBuilder.CreateIndex(
                name: "IX_occurrences_solicitacao_id",
                table: "occurrences",
                column: "solicitacao_id");

            migrationBuilder.CreateIndex(
                name: "IX_occurrences_tipo",
                table: "occurrences",
                column: "tipo");

            migrationBuilder.CreateIndex(
                name: "IX_pickup_requests_data_coleta_prevista",
                table: "pickup_requests",
                column: "data_coleta_prevista");

            migrationBuilder.CreateIndex(
                name: "IX_pickup_requests_numero_identificacao",
                table: "pickup_requests",
                column: "numero_identificacao",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pickup_requests_status",
                table: "pickup_requests",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_pickup_requests_usuario_id",
                table: "pickup_requests",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_ativo",
                table: "users",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "IX_users_cnpj",
                table: "users",
                column: "cnpj",
                unique: true,
                filter: "cnpj IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_tipo",
                table: "users",
                column: "tipo");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_ativo",
                table: "vehicles",
                column: "ativo");

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_placa",
                table: "vehicles",
                column: "placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assignments");

            migrationBuilder.DropTable(
                name: "occurrences");

            migrationBuilder.DropTable(
                name: "drivers");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "pickup_requests");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
