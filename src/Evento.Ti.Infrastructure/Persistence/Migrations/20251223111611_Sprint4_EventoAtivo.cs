using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Evento.Ti.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Sprint4_EventoAtivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eventos_ativos",
                columns: table => new
                {
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    AtivoId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_ativos", x => new { x.EventId, x.AtivoId });
                    table.ForeignKey(
                        name: "FK_eventos_ativos_ativos_AtivoId",
                        column: x => x.AtivoId,
                        principalTable: "ativos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_eventos_ativos_eventos_EventId",
                        column: x => x.EventId,
                        principalTable: "eventos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_eventos_ativos_AtivoId",
                table: "eventos_ativos",
                column: "AtivoId");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_ativos_EventId",
                table: "eventos_ativos",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eventos_ativos");
        }
    }
}
